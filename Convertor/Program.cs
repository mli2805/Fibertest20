using System;
using Iit.Fibertest.Graph;

namespace Convertor
{
    class Program
    {
        static void Main(string[] args)
        {
            Db db = new Db();

            new Migrator(db).Go();

            db.Save();

            Console.ReadLine();
        }
    }
}
