
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace DbExperiments
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new MyContext();

//            var user = new User() {Name = "superclient", Role = 5};
//            context.Users.Add(user);
//            context.SaveChanges();

            var users = context.Users.Where(u=>!u.IsEmailActivated).ToList();
            foreach (var user1 in users)
            {
                Console.WriteLine(user1.Name);
            }

            Console.WriteLine();

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
            foreach (var result in monitoringResults.Where(r=>r.Timestamp.LaterThan(new DateTime(2017,11,1,18,35,0))))
            {
                Console.WriteLine($"RTU {result.RtuId}      Trace {result.TraceId} ");
                Console.WriteLine($"Timestamp {result.Timestamp}");
            }
            Console.WriteLine("Done.");
            Console.ReadLine();
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
