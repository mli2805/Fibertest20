using System;
using System.Linq;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DbStartPoint
{
    class Program
    {
        static void Main()
        {
            //            SqliteExperiment();
            MySqlExperiment();


            Console.WriteLine("");
            Console.WriteLine("Done.");
            Console.ReadLine();
        }
        private static void SeedUsersTable(IFibertestDbContext dbContext)
        {
            if (dbContext.Users.Any(u => u.Role == Role.Developer) &&
                dbContext.Users.Any(u => u.Role == Role.Root))
                return; // seeded already

            var developer = new User() { Name = "developer", Password = "developer", Email = "", IsEmailActivated = false, Role = Role.Developer, IsDefaultZoneUser = true };
            dbContext.Users.Add(developer);
            var root = new User() { Name = "root", Password = "root", Email = "", IsEmailActivated = false, Role = Role.Root, IsDefaultZoneUser = true };
            dbContext.Users.Add(root);
            var oper = new User() { Name = "operator", Password = "operator", Email = "", IsEmailActivated = false, Role = Role.Operator, IsDefaultZoneUser = true };
            dbContext.Users.Add(oper);
            var supervisor = new User() { Name = "supervisor", Password = "supervisor", Email = "", IsEmailActivated = false, Role = Role.Supervisor, IsDefaultZoneUser = true };
            dbContext.Users.Add(supervisor);
            var superclient = new User() { Name = "superclient", Password = "superclient", Email = "", IsEmailActivated = false, Role = Role.Superclient, IsDefaultZoneUser = true };
            dbContext.Users.Add(superclient);
            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void PrintUsersTable(IFibertestDbContext dbContext)
        {
            var users = dbContext.Users.Where(u => !u.IsEmailActivated).ToList();
            foreach (var user in users)
            {
                Console.WriteLine($"{user.Name}  {user.Password}");
            }
            Console.WriteLine();
        }

        private static void MySqlExperiment()
        {
            var context = new MySqlContext();
            SeedUsersTable(context);
            PrintUsersTable(context);

            var clientStations = context.ClientStations.ToList();
            clientStations.Add(new ClientStation() {StationId = Guid.NewGuid()});
            context.SaveChanges();

        }
        private static void SqliteExperiment()
        {
            var context = new SqliteContext();
            SeedUsersTable(context);
            PrintUsersTable(context);
        }
    }
}
