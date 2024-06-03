using be_account.Data;
using be_account.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace TienDaoAPI.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DataContext _dbContext;
        internal DbSet<T> dbSet;

        public Repository(DataContext dbContext)
        {
            _dbContext = dbContext;
            dbSet = _dbContext.Set<T>();
        }

        public async Task<T?> CreateAsync(T entity)
        {
            dbSet.Add(entity);
            await SaveAsync();
            return entity;
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = dbSet;
            query.AsNoTracking();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task DeleteByIdAsync(int id)
        {
            T? entity = await dbSet.FindAsync(id);
            dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<T?> UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            dbSet.Update(entity);
            await SaveAsync();
            return entity;
        }
    }
}