using System.Data.Entity;
using MySql.Data.Entity;

namespace DbExperiments
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class MySqlContext : DbContext, IFibertestDbContext
    {
        public MySqlContext() : base("ftdb") { }

        public DbSet<User> Users { get; set; }
        public DbSet<MonitoringResult> MonitoringResults { get; set; }


        public new void SaveChanges()
        {
            base.SaveChanges();
        }
    }
}