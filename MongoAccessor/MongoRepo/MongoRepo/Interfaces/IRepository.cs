using HMTSolution.MongoAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HMTSolution.MongoRepo.Interfaces
{
    public interface IRepository<T, in TKey> where T : class, IEntity<TKey>, new() where TKey : IEquatable<TKey>
    {
        IQueryable<T> Get(Expression<Func<T, bool>>? predicate = null);
        Task<T> GetAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByIdAsync(TKey id);
        Task AddAsync(T entity, bool validate);
        Task<bool> AddRangeAsync(IEnumerable<T> entities);
        Task<bool> UpdateAsync(T entity, bool validate = true, bool overwriteServer = true);
        Task<T> UpdateAsync(T entity, Expression<Func<T, bool>> predicate);
        Task<T> DeleteAsync(T entity);
        Task<T> DeleteAsync(TKey id);
        Task<T> DeleteAsync(Expression<Func<T, bool>> predicate);
    }
}
