using BusinessObjects.Base;
using DataAccessObjects;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly SmartDietDbContext _context;
        public GenericRepository(SmartDietDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Set<T>().Attach(entity);
            }
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public void DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void DeleteRangeAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>()
                .Where(x => !x.DeletedTime.HasValue)
                .Where(predicate);

            // Add Includes
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Apply OrderBy
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public async Task<T> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>()
                .Where(x => !x.DeletedTime.HasValue)
                .Where(predicate);

            // Add Includes
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().Where(x => !x.DeletedTime.HasValue);

            // Add Includes
            query = ApplyIncludes(query, includes);

            // Apply OrderBy
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public async Task<BasePaginatedList<T>> GetAllWithPaginationAsync(
            int pageIndex,
            int pageSize,
    Expression<Func<T, bool>> searchTerm = null,
    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
    params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().Where(x => !x.DeletedTime.HasValue);

            // Apply search filter if provided
            if (searchTerm != null)
            {
                query = query.Where(searchTerm);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            int totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new BasePaginatedList<T>(items, totalCount, pageIndex, pageSize);
        }

        public async Task<T> GetByIdAsync(
            string id,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().Where(x => !x.DeletedTime.HasValue);

            // Add Includes
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            var keyName = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                  .Select(x => x.Name).Single();

            return await query.FirstOrDefaultAsync(entity => EF.Property<string>(entity, keyName) == id);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression, bool asNoTracking = true)
        {
            IQueryable<T> query = _context.Set<T>().Where(x => !x.DeletedTime.HasValue);
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.AnyAsync(expression);
        }

        public async Task UpdateAsync(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Set<T>().Attach(entity);
            }
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                if (_context.Entry(entity).State == EntityState.Detached)
                {
                    _context.Set<T>().Attach(entity);
                }
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public async Task<T> GetDeletedByIdAsync(string id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().Where(x => x.DeletedTime.HasValue);

            // Add Includes
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            var keyName = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                  .Select(x => x.Name).Single();

            return await query.FirstOrDefaultAsync(entity => EF.Property<string>(entity, keyName) == id);
        }

        public async Task<IEnumerable<T>> GetAllDeletedAsync(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().Where(x => x.DeletedTime.HasValue);

            // Add Includes
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Apply OrderBy
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public async Task<BasePaginatedList<T>> GetAllDeletedWithPaginationAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<T, bool>>? searchTerm = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().Where(x => x.DeletedTime.HasValue);

            if (searchTerm != null)
            {
                query = query.Where(searchTerm);
            }

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            int totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new BasePaginatedList<T>(items, totalCount, pageIndex, pageSize);
        }

        public async Task<BasePaginatedList<T>> FindWithPaginationAsync(
    int pageIndex,
    int pageSize,
    Expression<Func<T, bool>> predicate, // Specific predicate for filtering
    Expression<Func<T, bool>> searchTerm = null, // Optional search term for general filtering
    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, // Optional ordering
    params Expression<Func<T, object>>[] includes) // Optional includes
        {
            IQueryable<T> query = _context.Set<T>().Where(x => !x.DeletedTime.HasValue);

            // Apply the predicate filter
            query = query.Where(predicate);

            // Apply the search term filter if provided
            if (searchTerm != null)
            {
                query = query.Where(searchTerm);
            }

            // Add Includes
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Apply OrderBy if provided
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Get the total count of filtered items
            int totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<T>(items, totalCount, pageIndex, pageSize);
        }
        public async Task<IEnumerable<T>> FindDeletedAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>()
                .Where(x => x.DeletedTime.HasValue)
                .Where(predicate);

            // Add Includes
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Apply OrderBy
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public async Task<T> FirstOrDefaultDeletedAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>()
                .Where(x => x.DeletedTime.HasValue)
                .Where(predicate);

            // Add Includes
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        private IQueryable<T> ApplyIncludes(IQueryable<T> query, Expression<Func<T, object>>[] includes)
        {
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) =>
                {
                    if (include.Body is MemberInitExpression memberInit)
                    {
                        var path = string.Join(".", memberInit.Bindings.Select(b => b.Member.Name));
                        return current.Include(path);
                    }
                    return current.Include(include);
                });
            }
            return query;
        }
    }
}
