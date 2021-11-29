using HMTSolution.MongoRepo.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HMTSolution.MongoRepo.Interfaces
{
    public interface IStockRepository : IRepository<StockEntity, int>
    {
        Task<bool> UpsertVariantAsync(StockEntity entity, bool validate = true, bool overwriteServer = true);
        Task AddVariantAsync(StockEntity entity, bool validate = true);
    }
}
