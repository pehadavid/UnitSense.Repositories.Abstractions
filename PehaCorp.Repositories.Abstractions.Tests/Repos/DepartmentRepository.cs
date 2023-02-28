using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PehaCorp.CacheManagement;

namespace PehaCorp.Repositories.Abstractions.Tests.Repos
{
    public class DepartmentRepository : DataRepository<FakeDbContext, Departement, int, string>
    {
        public DepartmentRepository(RedisCacheManager redisCacheManager, LocalCacheManager localCacheManager,
            FakeDbContext dbCtx, RedisBusHandler busHandler, Type dataType, RepositorySetup setup, bool useCache = true)
            : base(redisCacheManager, localCacheManager, dbCtx, busHandler, dataType, setup)
        {
            this.UseDistributedCache = useCache;
            this.UseLocalCache = useCache;
            this.UseBus = useCache;
        }

        public override Task<Departement> GetByIdAsync(int key)
        {
            return FindDataAsync(GetPrimKeyValue(key),
                async () => { return await dbContext.Departements.FirstOrDefaultAsync(x => x.DepartmentId == key); });
        }

        public override Task<Departement> GetBySecondaryAsync(string key)
        {
            return FindDataAsync(GetSecondaryKeyValue(key),
                async () => { return await dbContext.Departements.FirstOrDefaultAsync(x => x.Name == key); });
        }

        public override async Task PutAsync(Departement data)
        {
            await dbContext.Departements.AddAsync(data);
            await dbContext.SaveChangesAsync();
            await WriteAllToCache(data);
        }

        public override async Task RefreshAsync(int key)
        {
            var item = await dbContext.Departements.FirstOrDefaultAsync(x => x.DepartmentId == key);
            await WriteAllToCache(item);
        }

        public override async Task DeleteAsync(int key)
        {
            var data = await dbContext.Departements.FirstOrDefaultAsync(x => x.DepartmentId == key);
            await DeleteAllFromCache(data);
            dbContext.Departements.Remove(data);
            await dbContext.SaveChangesAsync();
        }

        public override async Task UpdateAsync(Departement data)
        {
            var dbItem = await dbContext.Departements.FirstOrDefaultAsync(x => x.DepartmentId == data.DepartmentId);
            dbItem.CopyPropertiesFrom(data);
            await dbContext.SaveChangesAsync();
            await WriteAllToCache(dbItem);
        }

        protected override async Task WriteAllToCache(Departement data)
        {
            await WriteCacheDataAsync(GetPrimKeyValue(data.DepartmentId), data);
            await WriteCacheDataAsync(GetSecondaryKeyValue(data.Name), data);
        }

        protected override async Task DeleteAllFromCache(Departement data)
        {
            await DeleteCacheDataAsync(GetPrimKeyValue(data.DepartmentId), data);
            await DeleteCacheDataAsync(GetSecondaryKeyValue(data.Name), data);
        }
    }
}