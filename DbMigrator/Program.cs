using System;
using Iit.Fibertest.Graph;

namespace DbMigrator
{
    class Program
    {
        static void Main()
        {
            Db db = new Db();

            new Migrator(db).Go();

            db.Save();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
