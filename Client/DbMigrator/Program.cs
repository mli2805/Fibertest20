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


          
            new MainClass(iniFile, logFile).Go();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
