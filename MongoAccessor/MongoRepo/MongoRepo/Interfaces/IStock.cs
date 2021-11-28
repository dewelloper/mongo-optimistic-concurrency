using HMTSolution.MongoRepo.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.MongoRepo.Interfaces
{
    public interface IStockRepository : IRepository<StockEntity, int>
    {
    }
}
