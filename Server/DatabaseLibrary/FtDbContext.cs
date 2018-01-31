using System.Data.Entity;
using Iit.Fibertest.Dto;
using MySql.Data.Entity;

namespace Iit.Fibertest.DatabaseLibrary
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class FtDbContext : DbContext
    {
        private const string MySqlConnectionString = "server=localhost;user id=root;password=root;database=fibertest20";
        
        // to use another db engine just set another connection string
        public FtDbContext() : base(MySqlConnectionString) { }
        public FtDbContext(string connectionString) : base(connectionString) { }


        public DbSet<User> Users { get; set; }
        public DbSet<ClientStation> ClientStations { get; set; }
        public DbSet<RtuStation> RtuStations { get; set; }
        public DbSet<BaseRef> BaseRefs { get; set; }
        public DbSet<NetworkEvent> NetworkEvents { get; set; }
        public DbSet<BopNetworkEvent> BopNetworkEvents { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<SorFile> SorFiles { get; set; }
    }

    public interface ISettings
    {
        string MySqlConString { get; }
    }
    public class ServerSettings : ISettings
    {
        public string MySqlConString => "server=localhost;user id=root;password=root;database=fibertest20";
    }
    public class TestSettings : ISettings
    {
        public string MySqlConString => "server=localhost;user id=root;password=root;database=fibertest20fake";
    }
}