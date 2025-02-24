using BusinessObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(
            string id,
            Func<IQueryable<T>, IQueryable<T>> include = null,
            params Expression<Func<T, object>>[] includes);
        Task<T> GetDeletedByIdAsync(
            string id,
            params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync(
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IQueryable<T>> include = null,
            params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllDeletedAsync(
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes);
        Task<BasePaginatedList<T>> GetAllWithPaginationAsync(
            int pageIndex,
            int pageSize,
    Expression<Func<T, bool>> searchTerm = null,
    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
    Func<IQueryable<T>, IQueryable<T>> include = null,
    params Expression<Func<T, object>>[] includes);
        Task<BasePaginatedList<T>> FindWithPaginationAsync(
    int pageIndex,
    int pageSize,
    Expression<Func<T, bool>> predicate,
    Expression<Func<T, bool>> searchTerm = null,
    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
    Func<IQueryable<T>, IQueryable<T>> include = null,
    params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        void DeleteAsync(T entity);
        void DeleteRangeAsync(IEnumerable<T> entities);
        Task<IEnumerable<T>> FindAsync(
           Expression<Func<T, bool>> predicate,
           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
           Func<IQueryable<T>, IQueryable<T>> include = null,
           params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindDeletedAsync(
           Expression<Func<T, bool>> predicate,
           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
           params Expression<Func<T, object>>[] includes);
        Task<T> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>> include = null,
            params Expression<Func<T, object>>[] includes);
        Task<T> FirstOrDefaultDeletedAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);
        Task<bool> AnyAsync(Expression<Func<T, bool>> expression, bool asNoTracking = true);
    }
}
