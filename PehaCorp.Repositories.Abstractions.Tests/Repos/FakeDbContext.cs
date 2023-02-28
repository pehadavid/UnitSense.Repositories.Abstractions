using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PehaCorp.Repositories.Abstractions.Tests.Repos
{
    public class FakeDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Departement> Departements { get; set; }
     

        public FakeDbContext(DbContextOptions<FakeDbContext> options) : base(options)
        {
            
        }
    }

    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        public string FullName { get; set; }
        public double Salary { get; set; }
        public Departement Departement { get; set; }
        
        [ForeignKey("Departement")]
        public int? DepartmentId { get; set; }
        
    }

    public class Departement
    {
        [Key]
        public int DepartmentId { get; set; }

        public string Name { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
    
    public class DesignTimeFactory : IDesignTimeDbContextFactory<FakeDbContext>
    {
        public FakeDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<FakeDbContext>();
            builder.UseSqlite("Filename=test.sqlite");

            return new FakeDbContext(builder.Options);
        }
    }
}