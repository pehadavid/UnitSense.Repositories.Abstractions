using System;
using System.Threading.Tasks;
using MessagePack;

namespace UnitSense.Repositories.Abstractions.Filters
{

 
    public interface IQueryFilter<in TDbContext, TData> 
    {
        Task<FilteredDataSetResult<TData>> CreateGenTask(TDbContext dbContext);

        string GetUniqueKey()
        {
            var data = MessagePackSerializer.Serialize(this);
            return $"filtered:{BitConverter.ToString(data)}";
        }

    }
}