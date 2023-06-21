using Corelibs.Basic.Blocks;
using Corelibs.Basic.DDD;
using Corelibs.Basic.Repository;
using Microsoft.EntityFrameworkCore;

namespace Corelibs.EFCore
{
    public class DbContextRepository<TEntity, TEntityId> : IRepository<TEntity, TEntityId>, IReadRepository<TEntityId>
        where TEntity : class, IEntity<TEntityId>
    {
        private readonly DbContext _dbContext;

        public DbContextRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Result> Clear()
        {
            throw new NotImplementedException();
        }

        public async Task<Result> Delete(TEntityId id)
        {
            var result = Result.Success();

            var set = GetSet();
            TEntity entity = await set.FirstOrDefaultAsync(e => e.Id.ToString() == id.ToString());
            if (entity == null)
                return result.Fail();

            set.Remove(entity);

            var entriesAddedCount = await _dbContext.SaveChangesAsync();
            if (entriesAddedCount == 0)
                return Result.Failure();

            return result;
        }

        public Task<Result<bool>> ExistsOfName(string name, Func<TEntity, string> getName)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<TEntity[]>> GetAll()
        {
            var set = GetSet();
            TEntity[] entities = await set.ToArrayAsync();
            if (entities == null)
                return Result<TEntity[]>.Failure();

            return Result<TEntity[]>.Success(entities);
        }

        public Task<Result<TEntity[]>> GetAll(Action<int> setProgress, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<TEntity>> GetBy(TEntityId id)
        {
            var result = Result<TEntity>.Success();

            var set = GetSet();
            TEntity entity = await set.FirstOrDefaultAsync(e => e.Id.ToString() == id.ToString());
            if (entity == null)
                return result;

            return result.With(entity);
        }

        public async Task GetBy(TEntityId id, Result result)
        {
            var resultLocal = await GetBy(id);
            result.Add(resultLocal);
        }

        public async Task<Result<TEntity[]>> GetBy(IList<TEntityId> ids)
        {
            var result = Result<TEntity[]>.Success();

            var set = GetSet();
            TEntity[] entities = await set.Where(e => ids.Contains(e.Id)).ToArrayAsync();
            if (entities == null)
                return result.Fail();

            return Result<TEntity[]>.Success(entities);
        }

        public Task<Result<TEntity>> GetOfName(string name, Func<TEntity, string> getName)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> Save(TEntity item)
        {
            var set = GetSet();

            var entity = await set.FindAsync(item.Id.ToString());
            if (entity == null)
                await set.AddAsync(item);

            var entriesAddedCount = await _dbContext.SaveChangesAsync();
            return Result.Success();
        }

        private DbSet<TEntity> GetSet()
        {
            return _dbContext.Set<TEntity>();
        }
    }
}
