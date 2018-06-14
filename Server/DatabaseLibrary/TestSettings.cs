using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class TestSettings : ISettings
    {
        public void Init() { }
        public DbContextOptions<FtDbContext> Options => new DbContextOptionsBuilder<FtDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_database")
            .Options;
    }
}