using System;
using System.Collections.Generic;
using Iit.Fibertest.Graph;

namespace Convertor
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
