using System.Data.Entity;

namespace Iit.Fibertest.Client
{
    public class LocalDbSqliteContext : DbContext
    {
        public LocalDbSqliteContext(string connectionString) : base(connectionString) { }

        public DbSet<EsEvent> EsEvents { get; set; }
        public DbSet<EsSnapshot> EsSnapshots { get; set; }
    }
}