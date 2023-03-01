using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using PehaCorp.Repositories.Abstractions.Filters;

namespace PehaCorp.Repositories.Abstractions.Tests.Repos
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class EmployeeQueryFilter : RawFilter, IQueryFilter<FakeDbContext, Employee>
    {


        public int? DepartmentId { get; set; }



        public EmployeeQueryFilter()
        {

        }

        public async Task<FilteredDataSetResult<Employee>> CreateGenTask(FakeDbContext dbContext, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Employees.AsNoTracking();
            
            
            if (DepartmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == DepartmentId.Value);
            }

            var skipValue = (Page - 1) * Nb;
            var results = new FilteredDataSetResult<Employee>
            {
                NbPerPage = Nb,
                CurrentPage = Page,
                TotalItems = await query.CountAsync()
            };


            results.MaxPage = Convert.ToInt32(Math.Ceiling(results.TotalItems / (double)this.Nb));
            results.Results = await query.Skip(skipValue).Take(Nb).ToListAsync(cancellationToken: cancellationToken);
            return results;
        }

        public string GetUniqueKey()
        {
            var data = MessagePackSerializer.Serialize(this);
            return $"EmployeeQueryFilter:{BitConverter.ToString(data)}";
        }
    }
}