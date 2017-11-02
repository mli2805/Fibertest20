using System.Data.Entity;

namespace DbExperiments
{
    public class SqliteContext : DbContext, IFibertestDbContext
    {
        public SqliteContext() : base("sqlitedb") { }
        public DbSet<User> Users { get; set; }
        public DbSet<MonitoringResult> MonitoringResults { get; set; }
      
        public new void SaveChanges()
        {
            base.SaveChanges();
        }
    }
}
