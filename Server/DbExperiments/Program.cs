using System;
using System.IO;
using System.Linq;

namespace DbExperiments
{
    class Program
    {
        static void Main(string[] args)
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

            var developer = new User() {Name = "developer", Password = "developer", Email = "", IsEmailActivated = false, Role = Role.Developer, IsDefaultZoneUser = true};
            dbContext.Users.Add(developer);
            var root = new User() {Name = "root", Password = "root", Email = "", IsEmailActivated = false, Role = Role.Root, IsDefaultZoneUser = true };
            dbContext.Users.Add(root);
            var oper = new User() {Name = "operator", Password = "operator", Email = "", IsEmailActivated = false, Role = Role.Operator, IsDefaultZoneUser = true };
            dbContext.Users.Add(oper);
            var supervisor = new User() {Name = "supervisor", Password = "supervisor", Email = "", IsEmailActivated = false, Role = Role.Supervisor, IsDefaultZoneUser = true };
            dbContext.Users.Add(supervisor);
            var superclient = new User() {Name = "superclient", Password = "superclient", Email = "", IsEmailActivated = false, Role = Role.Superclient, IsDefaultZoneUser = true };
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

            SeedAndPrintMonitoringResults(context);
        }
        private static void SqliteExperiment()
        {
            var context = new SqliteContext();
            SeedUsersTable(context);
            PrintUsersTable(context);

            SeedAndPrintMonitoringResults(context);
        }

        private static void SeedAndPrintMonitoringResults(IFibertestDbContext context)
        {
            var someGuid = Guid.NewGuid();
            for (int i = 0; i < 10; i++)
            {
                var buffer = File.ReadAllBytes(@"c:\temp\123.sor");
                var monitoringResult = new MonitoringResult()
                {
                    RtuId = i % 2 == 0 ? Guid.NewGuid() : someGuid,
                    TraceId = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    SorBytes = buffer,
                };
                context.MonitoringResults.Add(monitoringResult);
            }
            context.SaveChanges();

            var monitoringResults = context.MonitoringResults.ToList();
            foreach (var result in monitoringResults.Where(r => r.Timestamp.LaterThan(new DateTime(2017, 11, 1, 18, 35, 0))))
            {
                Console.WriteLine($"RTU {result.RtuId}      Trace {result.TraceId} ");
                Console.WriteLine($"Timestamp {result.Timestamp}");
            }
        }
    }

    public static class DateTimeExt
    {
        public static bool LaterThan(this DateTime timestamp, DateTime anotherDateTime)
        {
            return timestamp > anotherDateTime;
        }
    }
}
