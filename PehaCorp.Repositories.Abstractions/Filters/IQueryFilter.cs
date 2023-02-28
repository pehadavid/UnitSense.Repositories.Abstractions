using System;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace PehaCorp.Repositories.Abstractions.Filters
{

 
    public interface IQueryFilter<in TDbContext, TData> 
    {
        Task<FilteredDataSetResult<TData>> CreateGenTask(TDbContext dbContext, CancellationToken cancellationToken = default);

        string GetUniqueKey()
        {
            var data = MessagePackSerializer.Serialize(this);
            return $"filtered:{BitConverter.ToString(data)}";
        }

    }
}