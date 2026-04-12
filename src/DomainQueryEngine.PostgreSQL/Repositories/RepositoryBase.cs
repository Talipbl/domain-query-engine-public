using DomainQueryEngine.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DomainQueryEngine.PostgreSQL
{
    public class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositoryBase(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
            => await _dbSet.FindAsync(id);

        public virtual async Task<List<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }
    }
}
