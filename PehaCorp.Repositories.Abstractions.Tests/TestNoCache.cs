using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PehaCorp.Repositories.Abstractions.Tests.Repos;
using Xunit;

namespace PehaCorp.Repositories.Abstractions.Tests
{
    public class TestNoCache : IClassFixture<RepositoryFixture>
    {
        RepositoryFixture fixture;
        private EmployeesRepository _employeesRepository;
        private DepartmentRepository _departmentRepository;

        public TestNoCache(RepositoryFixture fixture)
        {
            this.fixture = fixture;
            _employeesRepository = new EmployeesRepository(null, null, fixture.DbContext, null, typeof(Employee),
                fixture.Setup, false);
            _departmentRepository = new DepartmentRepository(null, null, fixture.DbContext, null, typeof(Departement),
                fixture.Setup, false);
        }
        [Fact]
        public async Task Create()
        {
            string name = "Jackson Boulon";
            Employee employee = new Employee()
            {
                FullName = name,
                Salary = 1000.01,
            };

           await _employeesRepository.PutAsync(employee);
           var dbItem = await fixture.DbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == employee.EmployeeId);
           Assert.True(employee.FullName == name && dbItem.FullName == name);
        }
        
        
        [Fact]
        public async Task CreateAndUpdate()
        {
            string name = "Josy Bala";
            Employee employee = new Employee()
            {
                FullName = name,
                Salary = 1500.01,
            };

            Departement departement = new Departement()
            {
                Name = "Sales"
            };
            
            await _employeesRepository.PutAsync(employee);
            await _departmentRepository.PutAsync(departement);

            employee.Departement = departement;
            await _employeesRepository.UpdateAsync(employee);

            var dbItem = await fixture
                .DbContext
                .Employees
                .Include(x => x.Departement)
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.EmployeeId);
            
            Assert.True(dbItem.Departement != null && dbItem.Departement.DepartmentId == departement.DepartmentId);
        }
        
        [Fact]
        public async Task CreateAndDelete()
        {
            string name = "John Grave";
            Employee employee = new Employee()
            {
                FullName = name,
                Salary = 2000.01,
            };
            await _employeesRepository.PutAsync(employee);

            await _employeesRepository.DeleteAsync(employee.EmployeeId);
            
            var dbItem = await fixture
                .DbContext
                .Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.EmployeeId);
            
            Assert.True(dbItem == null);
        }
        
        [Fact]
        public async Task CreateAndDeleteDependency()
        {
            string name = "Cary Danes";
            Employee employee = new Employee()
            {
                FullName = name,
                Salary = 2500.01,
            };
            
            Departement departement = new Departement()
            {
                Name = "Q/A"
            };
            await _employeesRepository.PutAsync(employee);
            await _departmentRepository.PutAsync(departement);
            await _employeesRepository.DeleteAsync(employee.EmployeeId);
            
            var dbItem = await fixture
                .DbContext
                .Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.EmployeeId);
            
            Assert.True(dbItem == null);
        }
    }
}