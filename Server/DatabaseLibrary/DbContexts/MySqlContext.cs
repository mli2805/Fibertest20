using System.Data.Entity;
using Iit.Fibertest.Dto;
using MySql.Data.Entity;

namespace Iit.Fibertest.DatabaseLibrary.DbContexts
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class MySqlContext : DbContext, IFibertestDbContext
    {
        public MySqlContext() : base("ftdb") { }

        public DbSet<User> Users { get; set; }

        public new void SaveChanges()
        {
            base.SaveChanges();
        }
    }
}