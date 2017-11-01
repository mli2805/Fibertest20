
using System;
using System.IO;
using System.Linq;

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

            byte[] buffer = null;
            buffer = File.ReadAllBytes(@"c:\temp\123.sor");
            var moniResultBlob = new MoniResultBlob()
            {
                IsFailed = true,
                DistanceToFirstBreak = 20.356,
                SorBytes = buffer,
            };
            using (MemoryStream ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
//                bw.Write(moniResultBlob);
            }

            var monitoringResult = new MonitoringResult()
            {
                RtuId = Guid.NewGuid(),
                TraceId = Guid.NewGuid(),
                Data = moniResultBlob,
            };
            context.MonitoringResults.Add(monitoringResult);
            context.SaveChanges();

            var monitoringResults = context.MonitoringResults.ToList();
            var result = monitoringResults.First();
            Console.WriteLine($"RTU {result.RtuId}      Trace {result.TraceId}");
            File.WriteAllBytes(@"c:\temp\789.sor", result.Data.SorBytes);

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
