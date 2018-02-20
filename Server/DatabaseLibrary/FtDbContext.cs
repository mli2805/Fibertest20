using Iit.Fibertest.Dto;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class FtDbContext : DbContext
    {
        public FtDbContext()  { }
        public FtDbContext(DbContextOptions<FtDbContext> options) : base(options) { }


        public DbSet<ClientStation> ClientStations { get; set; }
        public DbSet<RtuStation> RtuStations { get; set; }
        public DbSet<BaseRef> BaseRefs { get; set; }
        public DbSet<NetworkEvent> NetworkEvents { get; set; }
        public DbSet<BopNetworkEvent> BopNetworkEvents { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<SorFile> SorFiles { get; set; }
        public DbSet<User> Users { get; set; }
    }

    public interface ISettings
    {
        DbContextOptions<FtDbContext> Options { get; }

    }
    public class ServerSettings : ISettings
    {
        private string MySqlConnectionString => "server=localhost;user id=root;password=root;database=ft20efcore";

        public DbContextOptions<FtDbContext> Options =>
            new DbContextOptionsBuilder<FtDbContext>().UseMySql(MySqlConnectionString).Options;
    }
    public class TestSettings : ISettings
    {
        public DbContextOptions<FtDbContext> Options => new DbContextOptionsBuilder<FtDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_database")
            .Options;
    }
}