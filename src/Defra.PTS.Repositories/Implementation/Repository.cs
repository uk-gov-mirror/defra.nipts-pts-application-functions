using Defra.PTS.Application.Models.CustomException;
using Defra.PTS.Application.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Defra.PTS.Application.Repositories.Implementation
{
    [ExcludeFromCodeCoverageAttribute]
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected DbContext _dbContext;

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(TEntity entity)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity);
        }

        public DbContext Get_dbContext()
        {
            return _dbContext;
        }

        public void Delete(object id)
        {
            TEntity entity = _dbContext.Set<TEntity>().Find(id)!;
            if (entity != null)
            {
                _dbContext.Set<TEntity>().Remove(entity);                
            }
        }

        public TEntity Find(object id)
        {
            return _dbContext.Set<TEntity>().Find(id)!; 
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _dbContext.Set<TEntity>().ToList();
        }

        public void Remove(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
        }

        public void Update(TEntity entity)
        {
            _dbContext.Set<TEntity>().Update(entity);
        }
        public async Task<int> SaveChanges()
        {
            try {
            return await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new ApplicationFunctionException("An error occurred while saving changes to the database.", ex);
            }
        }
    }
}
