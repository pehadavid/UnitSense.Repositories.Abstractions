using System.IO;
using Microsoft.EntityFrameworkCore;
using PehaCorp.Repositories.Abstractions.Tests.Repos;

namespace PehaCorp.Repositories.Abstractions.Tests
{
    public class RepositoryFixture
    {
        public FakeDbContext DbContext { get; private set; }
        public RepositorySetup Setup { get; set; }

        public RepositoryFixture()
        {
            var builder = new DbContextOptionsBuilder<FakeDbContext>();
            File.Delete("test.sqlite");
            builder.UseSqlite("Filename=test.sqlite");

            DbContext = new FakeDbContext(builder.Options);
            DbContext.Database.Migrate();
            Setup = new RepositorySetup() { EnvironnementPrefix = "xUnit"};


        }
    }
}