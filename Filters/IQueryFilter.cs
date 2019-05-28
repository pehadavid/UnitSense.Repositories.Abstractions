using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UnitSense.Repositories.Abstractions.Filters
{
    public interface IQueryFilter<in TDbContext, TData> where TDbContext : DbContext
    {
        Task<FilteredDataSetResult<TData>> CreateGenTask(TDbContext dbContext);
        string GetUniqueKey();

    }
}