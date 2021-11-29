using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Endjin.Core.Retry;
using Endjin.Core.Retry.Policies;
using HMTSolution.Infrastructure.Settings;
using HMTSolution.MongoAccess.Exceptions;
using HMTSolution.MongoRepo.Entities;
using HMTSolution.MongoRepo.Interfaces;
using HMTSolution.MongoRepo.Strategies;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HMTSolution.MongoRepo.Repositories
{

    public class StockRepository : MongoRepositoryBase<StockEntity>, IStockRepository
    {

        protected new readonly IMongoCollection<StockEntity> _collection;
        private readonly MongoSettings _settings;
        private readonly MongoCollectionCounter _collectionCounter;
        public StockRepository(IOptions<MongoSettings> options) : base(options)
        {
            this._settings = options.Value;
            var client = new MongoClient(this._settings.ConnectionString);
            var db = client.GetDatabase(this._settings.Database);
            this._collection = db.GetCollection<StockEntity>(typeof(StockEntity).Name.ToLowerInvariant());

            _collectionCounter = new MongoCollectionCounter(typeof(StockEntity).Name, client, db);
        }

        public virtual async Task AddVariantAsync(StockEntity entity, bool validate = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (validate)
            {
                var validationResults = new List<ValidationResult>();
                var validationAttributes = new List<ValidationAttribute>();
                validationAttributes.Add(new CustomValidationAttribute(typeof(StockEntity), "ValidateColl"));

                if (!Validate(entity, ref validationResults))
                    throw new ValidationException(validationResults.FirstOrDefault(), validationAttributes.FirstOrDefault(), entity);
            }

            try
            {
                var currUpdateTime = entity.UpdateTime;
                entity.UpdateTime = DateTime.UtcNow;

                var isVariantedProductExist = await _collection.Find(_ => _.Id == entity.Id).FirstOrDefaultAsync();
                if (isVariantedProductExist != null && isVariantedProductExist.ProductCode == entity.ProductCode && isVariantedProductExist.VariantCode == entity.VariantCode)
                {
                    entity.Quantity = entity.Quantity + 1;

                    var res1 = await RetryTask<Task<ReplaceOneResult>>.Factory.StartNew(
                        () => _collection.WithWriteConcern(WriteConcern.WMajority).ReplaceOneAsync(arg =>
                                    (arg.Id == entity.Id) && (arg.UpdateTime == currUpdateTime),
                            SetUpdateTime(entity),
                            new ReplaceOptions { IsUpsert = true, BypassDocumentValidation = !validate },
                            CancellationToken.None), WriteRetryStrategy.Create(), new WriteRetryPolicy());
                }
                else
                {
                    //auto inc implemented
                    entity.Id = _collectionCounter.GetNextNumber();
                    await _collection.InsertOneAsync(entity);
                }

            }
            catch (Exception exception)
            {
                throw new MongoDataAccessException(exception.Message);
            }
        }

        public virtual async Task<bool> UpsertVariantAsync(StockEntity entity, bool validate = true, bool overwriteServer = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (validate)
            {
                var validationResults = new List<ValidationResult>();
                var validationAttributes = new List<ValidationAttribute>();
                validationAttributes.Add(new CustomValidationAttribute(typeof(StockEntity), "ValidateColl"));

                if (!Validate(entity, ref validationResults))
                    throw new ValidationException(validationResults.FirstOrDefault(), validationAttributes.FirstOrDefault(), entity);
            }

            var currUpdateTime = entity.UpdateTime;

            var isVariantedProductExist = await _collection.Find(_ => _.Id == entity.Id).FirstOrDefaultAsync();
            if (isVariantedProductExist.ProductCode == entity.ProductCode && isVariantedProductExist.VariantCode == entity.VariantCode)
                entity.Quantity = entity.Quantity + 1;

            ReplaceOneResult result;
            if (!overwriteServer)
                try
                {
                    var res1 = await RetryTask<Task<ReplaceOneResult>>.Factory.StartNew(
                        () => _collection.WithWriteConcern(WriteConcern.WMajority).ReplaceOneAsync(arg =>
                                    (arg.Id == entity.Id) && (arg.UpdateTime == currUpdateTime),
                            SetUpdateTime(entity),
                            new ReplaceOptions { IsUpsert = true, BypassDocumentValidation = !validate },
                            CancellationToken.None), WriteRetryStrategy.Create(), new WriteRetryPolicy());

                    result = res1.Result;
                }
                catch (Exception exception)
                // As overwriteServer was false, we are trying for optimistic concurrency the
                // query part of the replace method will return 0 docs as the UpdateTime is
                // not what we expect(that is a case where the state is different than expected).
                // 1 - Here, I followed a philosophy of versioning the data to be updated(I used the UpdateTime version).
                // 2 - Solution 2 possible to use id.
                {
                    var writeException = exception.InnerException as MongoWriteException;
                    //as tasks are going to throw aggregate exception.
                    if ((writeException != null) &&
                        (writeException.WriteError.Category == ServerErrorCategory.DuplicateKey))
                        throw new MongoConcurrencyException(entity.Id.ToString());

                    throw new MongoDataAccessException(exception.Message);
                }
            else
                try
                {
                    entity.Id = _collectionCounter.GetNextNumber();
                    var res2 = await RetryTask<Task<ReplaceOneResult>>.Factory.StartNew(
                        () => _collection.WithWriteConcern(WriteConcern.WMajority).ReplaceOneAsync(arg =>
                                    arg.Id == entity.Id,
                            SetUpdateTime(entity),
                            new ReplaceOptions { IsUpsert = true, BypassDocumentValidation = !validate },
                            CancellationToken.None), WriteRetryStrategy.Create(), new AnyException());
                    result = res2.Result;
                }
                catch (Exception exception)
                {
                    throw new MongoDataAccessException(exception.Message);
                }
            return result.IsAcknowledged;
        }


    }
}

