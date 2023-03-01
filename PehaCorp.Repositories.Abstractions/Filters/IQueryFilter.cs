using System;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace PehaCorp.Repositories.Abstractions.Filters
{

 
    /// <summary>
    /// Interface for query filters and task to be executed on the database
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public interface IQueryFilter<in TDbContext, TData> 
    {
        /// <summary>
        /// Task to be executed on the database
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FilteredDataSetResult<TData>> CreateGenTask(TDbContext dbContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a unique key for the filter
        /// </summary>
        /// <returns></returns>
        string GetUniqueKey();
        // {
        //     var data = MessagePackSerializer.Serialize(this);
        //     return $"filtered:{BitConverter.ToString(data)}";
        // }

    }
}