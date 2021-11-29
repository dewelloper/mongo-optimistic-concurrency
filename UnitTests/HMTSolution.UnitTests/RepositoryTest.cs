using HMTSolution.BCS.Controllers;
using HMTSolution.BCS.Models.Requests;
using HMTSolution.BCS.Validations.Resolver;
using HMTSolution.BCS.Validations.StockValidation;
using HMTSolution.Infrastructure.Settings;
using HMTSolution.MongoAccess.Exceptions;
using HMTSolution.MongoRepo.Entities;
using HMTSolution.MongoRepo.Interfaces;
using HMTSolution.MongoRepo.Repositories;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HMTSolution.UnitTests
{
    public class StockControllerTests
    {
        private readonly Mock<ILogger<StockController>> logger;
        //private readonly Mock<IMapper> mapper;
        private readonly Mock<IValidatorResolver> validatorResolver;
        private readonly StockRepository stockRepository;


        public StockControllerTests()
        {
            logger = new Mock<ILogger<StockController>>();
            //mapper = new Mock<IMapper>();
            validatorResolver = new Mock<IValidatorResolver>();

            logger.Setup(
               l => l.Log(
                   It.IsAny<LogLevel>(),
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
               .Callback((IInvocation invocation) =>
               {
               });

            MongoSettings app = new MongoSettings() { ConnectionString = "mongodb://testuser:Deneme123@127.0.0.1:27017/StockDb", Database= "db" };
            var mock = new Mock<IOptions<MongoSettings>>();
            mock.Setup(ap => ap.Value).Returns(app);

            stockRepository = new StockRepository(mock.Object);
        }

        
        [Fact]
        public void RepoGet_Test()
        {
            var actual = stockRepository.Get();

            Assert.True(actual.ToList().Count >= 0);
        }

        [Theory]
        [InlineData(typeof(IQueryable<StockEntity>))]
        public async Task RepoGetAsync_Test(Type expected)
        {
            var actual = await stockRepository.GetAsync(_=> _.ProductCode != null);

            Assert.Equal(expected, actual.GetType());
        }

        [Fact]
        public void test_upsert_async_concurrency()
        {
            IEnumerable<StockEntity> list = stockRepository.Get().ToList();
            //Assert.Single(list);

            var listFirst = list.First();

            var stock = new StockEntity
            {
                Id = listFirst.Id,
                UpdateTime = list.First().UpdateTime,
                ProductCode = "A",
                VariantCode = "A001",
                Quantity = 1,
            };

            bool success = true;
            try
            {
                var res = stockRepository.UpdateAsync(stock, true, false).Result;
            }
            catch (Exception ex)
            {
                success = false;
            }
            Assert.True(success);

            var stock2 = new StockEntity
            {
                Id = listFirst.Id,
                UpdateTime = DateTime.Now,
                ProductCode = "B",
                VariantCode = "B001",
                Quantity = 5,
            };

            try
            {
                var res1 = stockRepository.UpdateAsync(stock2, true, false).Result;
            }
            catch (Exception excep)
            {
                Assert.Equal("MongoConcurrencyException", excep.InnerException.GetType().Name);
            }
        }

    }
}