using System.Data.Entity.Migrations;
using Iit.Fibertest.DatabaseLibrary.DbContexts;

namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MySqlContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Iit.Fibertest.DatabaseLibrary.DbContexts.MySqlContext";
        }

        protected override void Seed(MySqlContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
