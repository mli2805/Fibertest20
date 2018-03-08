using System;

namespace Iit.Fibertest.DbMigrator
{
    class Program
    {
        static void Main()
        {
            var graph = new Graph();
            var fileStringParser = new FileStringParser(graph);
            var fileStringTraceParser = new FileStringTraceParser(graph);
            new Migrator(graph, fileStringParser,fileStringTraceParser).Go();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
