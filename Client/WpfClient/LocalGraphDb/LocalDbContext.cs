using System.Data.Entity;

namespace Iit.Fibertest.Client
{
    public class LocalDbContext : DbContext
    {
        public LocalDbContext() : base(@"sqlitedb") { }

        public DbSet<EsEvent> EsEvents { get; set; }
    }
}