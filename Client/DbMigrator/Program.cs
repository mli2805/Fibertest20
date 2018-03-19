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


            var graphModel = new GraphModel();
            var fileStringParser = new FileStringParser(graphModel);
            var fileStringTraceParser = new FileStringTraceParser(graphModel);
            var graphFetcher = new GraphFetcher(logFile, graphModel, fileStringParser, fileStringTraceParser);
            var sorFetcher = new SorFetcher("172.16.4.115"); // server with old base
            new MainClass(iniFile, logFile, graphModel, graphFetcher, sorFetcher).Go();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
