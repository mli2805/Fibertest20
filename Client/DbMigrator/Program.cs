using System;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DbMigrator
{
    class Program
    {
        static void Main()
        {
            var iniFile = new IniFile();
            iniFile.AssignFile("migrator.ini");
            var logFile = new LogFile(iniFile);
            logFile.AssignFile("migrator.log");

            var graph = new Graph();
            var fileStringParser = new FileStringParser(graph);
            var fileStringTraceParser = new FileStringTraceParser(graph);
            new Migrator(iniFile, logFile, graph, fileStringParser,fileStringTraceParser).Go();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
