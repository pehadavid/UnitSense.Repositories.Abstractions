using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PehaCorp.CacheManagement;

namespace PehaCorp.Repositories.Abstractions.Tests.Repos
{
    public class EmployeesRepository : DataRepository<FakeDbContext, Employee, int, string>
    {
        public EmployeesRepository(RedisCacheManager redisCacheManager, LocalCacheManager localCacheManager, FakeDbContext dbCtx, RedisBusHandler busHandler, Type dataType, RepositorySetup setup, bool useCache = true) 
            : base(redisCacheManager, localCacheManager, dbCtx, busHandler, dataType, setup)
        {
            this.UseDistributedCache = useCache;
            this.UseLocalCache = useCache;
            this.UseBus = useCache;
        }

        public override Task<Employee> GetByIdAsync(int key, CancellationToken cancellationToken = default)
        {
            return FindDataAsync(GetPrimKeyValue(key), async () =>
            {
                return await dbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == key);
            });
        }

     

        public override Task<Employee> GetBySecondaryAsync(string key, CancellationToken cancellationToken = default)
        {
            return FindDataAsync(GetSecondaryKeyValue(key), async () =>
            {
                return await dbContext.Employees.FirstOrDefaultAsync(x => x.FullName == key, cancellationToken: cancellationToken);
            }, cancellationToken);
        }

        public override async Task PutAsync(Employee data, CancellationToken cancellationToken = default)
        {
            await dbContext.Employees.AddAsync(data, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await WriteAllToCache(data);
            
        }

        public override async Task RefreshAsync(int key, CancellationToken cancellationToken = default)
        {
            var item = await dbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == key, cancellationToken: cancellationToken);
            await WriteAllToCache(item);
        }

        public override async Task DeleteAsync(int key, CancellationToken cancellationToken = default)
        {
            var data = await dbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == key, cancellationToken: cancellationToken);
            await DeleteAllFromCache(data);
            dbContext.Employees.Remove(data);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public override async Task UpdateAsync(Employee data, CancellationToken cancellationToken = default)
        {
            var dbItem = await dbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == data.EmployeeId, cancellationToken: cancellationToken);
            dbItem.CopyPropertiesFrom(data);
            await dbContext.SaveChangesAsync(cancellationToken);
            await WriteAllToCache(dbItem);
        }

        protected override async Task WriteAllToCache(Employee data)
        {
        
            await WriteCacheDataAsync(GetPrimKeyValue(data.EmployeeId), data);
            await WriteCacheDataAsync(GetSecondaryKeyValue(data.FullName), data);
        }

        protected override async Task DeleteAllFromCache(Employee data)
        {
            await DeleteCacheDataAsync(GetPrimKeyValue(data.EmployeeId), data);
            await DeleteCacheDataAsync(GetSecondaryKeyValue(data.FullName), data);
        }
    }
}