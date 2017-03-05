using System;
using Iit.Fibertest.Graph;

namespace DbMigrator
{
    class Program
    {
        static void Main()
        {
            Db db = new Db();
            db.Events.Clear();
            new Migrator(db).Go();

            db.Save();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
