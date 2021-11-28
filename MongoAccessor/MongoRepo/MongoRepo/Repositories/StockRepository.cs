using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HMTSolution.Infrastructure.Settings;
using HMTSolution.MongoRepo.Entities;
using HMTSolution.MongoRepo.Interfaces;
using Microsoft.Extensions.Options;

namespace HMTSolution.MongoRepo.Repositories
{

    public class StockRepository : MongoRepositoryBase<StockEntity>, IStockRepository
    {
        public StockRepository(IOptions<MongoSettings> options) : base(options)
        {
        }
    }
}

