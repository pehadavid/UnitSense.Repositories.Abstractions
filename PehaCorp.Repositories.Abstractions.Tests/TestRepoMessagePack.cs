using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using PehaCorp.Repositories.Abstractions.Filters;
using PehaCorp.Repositories.Abstractions.Tests.Repos;
using Xunit;

namespace PehaCorp.Repositories.Abstractions.Tests;

public class TestRepoMessagePack : IClassFixture<RepositoryFixture>

{
    
    private EmployeesRepository _employeesRepository;

    public TestRepoMessagePack(RepositoryFixture fixture)
    {
        _employeesRepository = fixture.EmployeesRepository;
    }
 
    [Fact]
    public async Task TestSerialize()
    {
        var filters = new EmployeeQueryFilter() { DepartmentId = 10 };
       var data = await _employeesRepository.GetListAsync(filters: filters, cancellationToken: CancellationToken.None);
    }
}