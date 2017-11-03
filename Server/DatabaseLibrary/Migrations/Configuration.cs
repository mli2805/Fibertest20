namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<Iit.Fibertest.DatabaseLibrary.DbContexts.MySqlContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Iit.Fibertest.DatabaseLibrary.DbContexts.MySqlContext";
        }

        protected override void Seed(Iit.Fibertest.DatabaseLibrary.DbContexts.MySqlContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
