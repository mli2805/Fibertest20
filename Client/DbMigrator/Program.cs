using System;
using Iit.Fibertest.Graph;
using Serilog;

namespace Iit.Fibertest.DbMigrator
{
    class Program
    {
        static void Main()
        {
            //Db db = new Db(new LoggerConfiguration().WriteTo.Console().CreateLogger());
            //db.Events.Clear();
            //new Migrator(db).Go();

            //db.Save();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
