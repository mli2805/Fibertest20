using System.Data.Entity;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DbLibrary.DbContexts
{
    public class SqliteContext : DbContext, IFibertestDbContext
    {
        public SqliteContext() : base("sqlitedb") { }
        public DbSet<User> Users { get; set; }
      
        public new void SaveChanges()
        {
            base.SaveChanges();
        }
    }
}
