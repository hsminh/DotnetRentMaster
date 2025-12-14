using RentMaster.Addresses.Models;
using RentMaster.Core.Repositories.Interface;

namespace RentMaster.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using RentMaster.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
        
        public virtual IEnumerable<T> Filter(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }
        
        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }


        public virtual async Task<T?> FindByUidAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> FilterAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(bool includeDeleted = false)
        {
            // Optional: If entity has IsDelete property
            if (!includeDeleted && typeof(T).GetProperty("IsDelete") != null)
            {
                var param = Expression.Parameter(typeof(T), "x");
                var prop = Expression.Property(param, "IsDelete");
                var cond = Expression.Equal(prop, Expression.Constant(false));
                var lambda = Expression.Lambda<Func<T, bool>>(cond, param);
                return await _dbSet.Where(lambda).ToListAsync();
            }

            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }       

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
        
    }