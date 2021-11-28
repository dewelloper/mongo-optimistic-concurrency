using Endjin.Core.Retry;
using Endjin.Core.Retry.Policies;
using HMTSolution.Infrastructure.Settings;
using HMTSolution.MongoAccess.Abstracts;
using HMTSolution.MongoAccess.Exceptions;
using HMTSolution.MongoRepo.Entities;
using HMTSolution.MongoRepo.Interfaces;
using HMTSolution.MongoRepo.Strategies;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//Here, async methods are used against blocking.

// On the concurrency side, methods such as FindOneAndReplaceAsync, which replace the FindAndModify methods that come with Mongo C# Driver 2.0, 
// which meet atomic operations, are used.
// I used optimistic concurrency on some methods for this study case below

namespace HMTSolution.MongoRepo
{
    // UoW not implemented but it would be in features
    public abstract class MongoRepositoryBase<T> : IRepository<T, int> where T : MongoEntityBase, new()
    {
        protected readonly IMongoCollection<T> _collection;
        private readonly MongoSettings _settings;
        private readonly MongoCollectionCounter _collectionCounter;
        protected MongoRepositoryBase(IOptions<MongoSettings> options)
        {
            this._settings = options.Value;
            var client = new MongoClient(this._settings.ConnectionString);
            var db = client.GetDatabase(this._settings.Database);
            this._collection = db.GetCollection<T>(typeof(T).Name.ToLowerInvariant());

            _collectionCounter = new MongoCollectionCounter(typeof(T).Name, client, db);
        }

        public virtual IQueryable<T> Get(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null
                ? _collection.AsQueryable()
                : _collection.AsQueryable().Where(predicate);
        }

        public virtual Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return _collection.Find(predicate).FirstOrDefaultAsync();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _collection.Find(_ => _.Id == id).SingleOrDefaultAsync();
        }

        //Concurrency implemented
        public virtual async Task AddAsync(T entity, bool validate = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (validate)
            {
                var validationResults = new List<ValidationResult>();
                var validationAttributes = new List<ValidationAttribute>();
                validationAttributes.Add(new CustomValidationAttribute(typeof(T), "ValidateColl"));

                if (!Validate(entity, ref validationResults))
                    throw new ValidationException(validationResults.FirstOrDefault(), validationAttributes.FirstOrDefault(), entity);
            }

            try
            {
                //auto inc implemented
                entity.Id = _collectionCounter.GetNextNumber();
                await RetryTask.Factory.StartNew(
                    () => _collection.WithWriteConcern(WriteConcern.WMajority).InsertOneAsync(
                        entity, new InsertOneOptions { BypassDocumentValidation = !validate }),
                    CancellationToken.None, WriteRetryStrategy.Create(), new AnyException());
            }
            catch (Exception exception)
            {
                throw new MongoDataAccessException(exception.Message);
            }
        }

        public virtual async Task<bool> AddRangeAsync(IEnumerable<T> entities)
        {
            var options = new BulkWriteOptions { IsOrdered = false, BypassDocumentValidation = false };

            foreach(var entity in entities)
            {
                entity.Id = _collectionCounter.GetNextNumber();
            }

            return (await _collection.BulkWriteAsync((IEnumerable<WriteModel<T>>)entities, options)).IsAcknowledged;
        }

        public virtual async Task<bool> UpdateAsync(int id, T entity, bool validate = true, bool overwriteServer = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (validate)
            {
                var validationResults = new List<ValidationResult>();
                var validationAttributes = new List<ValidationAttribute>();
                validationAttributes.Add(new CustomValidationAttribute(typeof(T), "ValidateColl"));

                if (!Validate(entity, ref validationResults))
                    throw new ValidationException(validationResults.FirstOrDefault(), validationAttributes.FirstOrDefault(), entity);
            }

            var currUpdateTime = entity.UpdateTime;

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

        public virtual async Task<T> UpdateAsync(T entity, Expression<Func<T, bool>> predicate)
        {
            entity.Id = _collectionCounter.GetNextNumber();
            return await _collection.FindOneAndReplaceAsync(predicate, entity);
        }

        public virtual async Task<T> DeleteAsync(T entity)
        {
            return await _collection.FindOneAndDeleteAsync(x => x.Id == entity.Id);
        }

        public virtual async Task<T> DeleteAsync(int id)
        {
            return await _collection.FindOneAndDeleteAsync(x => x.Id == id);
        }

        public virtual async Task<T> DeleteAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.FindOneAndDeleteAsync(filter);
        }

        private bool Validate(T document, ref List<ValidationResult> errors)
        {
            try
            {
                var validationContext = new ValidationContext(document, null, null);

                var isValid = Validator.TryValidateObject(document, validationContext, errors, true);

                return isValid;
            }
            catch (Exception exception)
            {
                throw new MongoDataAccessException(exception.Message);
            }
        }

        private T SetUpdateTime(T entity)
        {
            entity.UpdateTime = DateTime.UtcNow;
            return entity;
        }
    }
}

