using System.Data.Entity.Migrations;

namespace DbExperiments.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MySqlContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "DbExperiments.MySqlContext";
        }

        protected override void Seed(MySqlContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
