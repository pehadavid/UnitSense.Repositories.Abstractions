using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnitSense.CacheManagement;
using UnitSense.Repositories.Abstractions.Filters;

namespace UnitSense.Repositories.Abstractions
{
    /// <summary>
    /// Allow to handle data with traversal cache management
    /// </summary>
    /// <typeparam name="TData">Prmiary type to handle</typeparam>
    /// <typeparam name="TKey">Primary Key Type</typeparam>
    /// <typeparam name="TSecondKey">Index/secondary key type</typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public abstract class DataRepository<TDbContext, TData, TKey, TSecondKey>
        where TData : class 
    {
        protected RedisCacheManager redisCacheManager;
        protected LocalCacheManager localCacheManager;
        protected RedisBusHandler busHandler;
        protected RepositorySetup setup;
        protected string typeName;
        protected string hashSetKey;
        protected static string mainPrefix => "_repositoryManager";
#if DEBUG
        /// <summary>
        /// time to live (long value) in cache
        /// </summary>
        protected TimeSpan lngTs => TimeSpan.FromMinutes(180);

        /// <summary>
        /// time to live (short value) in cache
        /// </summary>
        protected TimeSpan shortTs => TimeSpan.FromMinutes(30);
#else
        /// <summary>
        /// time to live (long value) in cache
        /// </summary>
        protected TimeSpan lngTs => TimeSpan.FromMinutes(180);
        /// <summary>
        /// time to live (short value) in cache
        /// </summary>
        protected TimeSpan shortTs => TimeSpan.FromMinutes(30);
#endif
        /// <summary>
        /// DbContext from data fetching
        /// </summary>
        protected TDbContext dbContext;

        /// <summary>
        /// allow to set redis cache active / inactive (default true)
        /// </summary>
        public bool UseDistributedCache { get; set; }

        /// <summary>
        /// allow to set local memory cache active / inactive (default true)
        /// </summary>
        public bool UseLocalCache { get; set; }

        /// <summary>
        /// Propagate value across redis bus (default true)
        /// </summary>
        public bool UseBus { get; set; }

        protected DataRepository(RedisCacheManager redisCacheManager, LocalCacheManager localCacheManager,
            TDbContext dbCtx, RedisBusHandler busHandler, Type dataType, RepositorySetup setup)
        {
            this.redisCacheManager = redisCacheManager;
            this.localCacheManager = localCacheManager;
            this.dbContext = dbCtx;
            this.typeName = dataType.FullName;
            this.hashSetKey = $"hashset-{typeName}-{setup.EnvironnementPrefix}";
            this.busHandler = busHandler;
            this.setup = setup;

            UseDistributedCache = true;
            UseLocalCache = true;
            UseBus = true;
        }

        /// <summary>
        /// morph cache key to a normalized one
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string MorphKey(string key)
        {
            return $"{mainPrefix}:{setup.EnvironnementPrefix}:{typeName}:{key}";
        }

        /// <summary>
        /// Get primary key as string value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string GetPrimKeyValue(TKey key)
        {
            return $"{key}";
        }

        /// <summary>
        /// Get secondary key as string value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string GetSecondaryKeyValue(TSecondKey key)
        {
            return $"secondary:{key}";
        }

        #region I/O

        /// <summary>
        /// Find data across all available data subsystems
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataGeneratorAsync"></param>
        /// <returns></returns>
        protected  async Task<TData> FindDataAsync<TData>(string key, Func<Task<TData>> dataGeneratorAsync)
            where TData : class
        {
            var morphedKey = MorphKey(key);
            if (!localCacheManager.GetByKey(morphedKey, out TData dataSet) || !UseLocalCache)
            {
                try
                {
                    Debug.WriteLine($"{this.GetType().FullName} ({morphedKey}) local cache miss or disabled");
                    if (!redisCacheManager.GetByKey<TData>(morphedKey, out dataSet) || !UseDistributedCache)
                    {
                        Debug.WriteLine($"{this.GetType().FullName} ({morphedKey}) redis cache miss");
                        dataSet = await dataGeneratorAsync();
                        if (UseDistributedCache)
                            redisCacheManager.SetValue(morphedKey, dataSet, lngTs);
                    }

                    if (UseLocalCache)
                        localCacheManager.SetValue(morphedKey, dataSet, shortTs);
                }
                catch (Exception e)
                {
                    return await dataGeneratorAsync();
                }
            }

            return dataSet;
        }

        /// <summary>
        /// Find data across all available data cache subsystems
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="hashsetKey"></param>
        /// <param name="itemKey"></param>
        /// <param name="dataGeneratorAsync"></param>
        /// <returns></returns>
        protected async Task<TData> FindDataHashetAsync<TData>(string itemKey,
            Func<Task<TData>> dataGeneratorAsync) where TData : class
        {
            try
            {
                var data = localCacheManager.HashGetByKey<TData>(hashSetKey, itemKey);
                if (data == null || !UseLocalCache)
                {
                    Debug.WriteLine(
                        $"{this.GetType().FullName} ({hashSetKey}/ {itemKey}) local cache miss or disabled");
                    data = redisCacheManager.HashGetByKey<TData>(hashSetKey, itemKey);
                    if (data == null || !UseDistributedCache)
                    {
                        Debug.WriteLine($"{this.GetType().FullName} ({hashSetKey}/{itemKey}) redis cache miss");
                        data = await dataGeneratorAsync();
                        if (UseDistributedCache)
                            redisCacheManager.HashSetByKey(hashSetKey, itemKey, data);
                    }

                    if (UseLocalCache)
                        localCacheManager.HashSetByKey(hashSetKey, itemKey, data);
                }

                return data;
            }
            catch (Exception e)
            {
                return await dataGeneratorAsync();
            }
        }

        /// <summary> 
        /// Write data across all cache data subsystems
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected Task WriteCacheDataAsync(string key, object item)
        {
            var morphedKey = MorphKey(key);
            this.redisCacheManager.SetValue(morphedKey, item, lngTs);
            redisCacheManager.DeleteHashSet(hashSetKey);
            localCacheManager.DeleteHashSet(hashSetKey);
            return busHandler.PublishAsync(JsonConvert.SerializeObject(
                new BroadcastItem(item, BroadcastOperation.WRITE, hashSetKey) {Key = morphedKey},
                RedisCacheManager.GetJsonSerializerSettings()));
        }
        
        
      
        /// <summary>
        /// Delete data across all cache only subsystems
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected Task DeleteCacheDataAsync(string key, TData item)
        {
            var morphedKey = MorphKey(key);
            var redisTask = redisCacheManager.DeleteAsync(morphedKey);
            var localTask = localCacheManager.DeleteAsync(morphedKey);
            redisCacheManager.DeleteHashSet(hashSetKey);
            localCacheManager.DeleteHashSet(hashSetKey);
            var broadcastTask = busHandler.PublishAsync(JsonConvert.SerializeObject(
                new BroadcastItem(item, BroadcastOperation.DELETE, hashSetKey) {Key = morphedKey},
                RedisCacheManager.GetJsonSerializerSettings()));
            return Task.WhenAll(redisTask, localTask, broadcastTask);
        }

        #endregion


        #region iface

        /// <summary>
        /// get <see cref="TData"/> using primary key (single element)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual TData GetById(TKey key)
        {
            return GetByIdAsync(key).GetAwaiter().GetResult();
        }

        /// <summary>
        /// get <see cref="TData"/> using primary key (single element, asynchronous)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract Task<TData> GetByIdAsync(TKey key);


        /// <summary>
        /// get a collection of <see cref="TData"/> using a <see cref="IQueryFilter"/> model
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public virtual Task<FilteredDataSetResult<TData>> GetListAsync(IQueryFilter<TDbContext, TData> filters)
        {
            Func<Task<FilteredDataSetResult<TData>>> funcGen = () => filters.CreateGenTask(dbContext);
            return FindDataHashetAsync(filters.GetUniqueKey(), funcGen);
        }

        /// <summary>   
        /// get <see cref="TData"/> using secondary key (single element)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual TData GetBySecondary(TSecondKey key)
        {
            return GetBySecondaryAsync(key).GetAwaiter().GetResult();
        }

        /// <summary>
        /// get <see cref="TData"/> using secondary key (single element, asynchronous)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract Task<TData> GetBySecondaryAsync(TSecondKey key);


        /// <summary>
        /// put <see cref="TData"/> across all data subsystems
        /// </summary>
        /// <param name="data"></param>
        public virtual void Put(TData data)
        {
            PutAsync(data).GetAwaiter().GetResult();
        }

        /// <summary> 
        /// put <see cref="TData"/> across all data subsystems (asynchronous)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Task PutAsync(TData data);


        /// <summary>
        /// Refresh all cache subsystem from database
        /// </summary>
        /// <param name="key"></param>
        public abstract Task RefreshAsync(TKey key);

        /// <summary>
        /// Refresh all cache subsystem from database, then get result
        /// </summary>
        /// <param name="key"></param>
        public virtual Task<TData> RefreshAndGetAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// delete <see cref="TData"/> across all data subsystems
        /// </summary>
        /// <param name="key"></param>
        public virtual void Delete(TKey key)
        {
            DeleteAsync(key).GetAwaiter().GetResult();
        }

        /// <summary>
        /// delete <see cref="TData"/> across all data subsystems (asynchronous)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract Task DeleteAsync(TKey key);

        /// <summary>
        /// update <see cref="TData"/> across all data subsystems
        /// </summary>
        /// <param name="data"></param>
        public virtual void Update(TData data)
        {
            UpdateAsync(data).GetAwaiter().GetResult();
        }

        /// <summary>
        /// update <see cref="TData"/> across all data subsystems (asynchronous)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Task UpdateAsync(TData data);

        /// <summary>
        /// write <see cref="TData"/> across all cache subsystems
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract Task WriteAllToCache(TData data);

        /// <summary>
        ///  delete <see cref="TData"/> across all cache subsystems
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract Task DeleteAllFromCache(TData data);

        #endregion


        /// <summary>
        /// Delete all resources in cache subsystems
        /// </summary>
        public async Task FlushAsync()
        {
            this.localCacheManager.Clear();
            var db = this.redisCacheManager.GetMultiplexer().GetDatabase();
            var redisResult = db.Execute($"KEYS *{mainPrefix}:{setup.EnvironnementPrefix}:*");
        }

        public virtual void Dispose()
        {
        }
    }
}