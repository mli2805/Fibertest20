using System.Data.Entity;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using MySql.Data.Entity;

namespace Iit.Fibertest.DatabaseLibrary.DbContexts
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class MySqlContext : DbContext, IFibertestDbContext
    {
        public MySqlContext() : base("server=localhost;user id=root;password=root;database=fibertest20") { }

        public DbSet<User> Users { get; set; }
        public DbSet<ClientStation> ClientStations { get; set; }
        public DbSet<RtuStation> RtuStations { get; set; }
        public DbSet<BaseRef> BaseRefs { get; set; }
        public DbSet<NetworkEvent> NetworkEvents { get; set; }
        public DbSet<OpticalEvent> OpticalEvents { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<SorFile> SorFiles { get; set; }

        public new void SaveChanges()
        {
            base.SaveChanges();
        }

        public new async Task<int> SaveChangesAsync()
        {
           return await base.SaveChangesAsync();
        }
    }
}