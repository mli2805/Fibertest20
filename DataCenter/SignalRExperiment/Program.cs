using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.FtSignalRClientLib;
using Iit.Fibertest.UtilsLib;

namespace SignalRExperiment
{
    class Program
    {
        static void Main()
        {
            var iniFile = new IniFile();
            iniFile.AssignFile("signalRExperiment.ini");
            var logFile = new LogFile(iniFile);
            logFile.AssignFile("signalRExperiment.log");
            var signalRClient = new FtSignalRClient(iniFile, logFile);
            signalRClient.Build();
            signalRClient.Connect().Wait();

            Console.WriteLine("Press any key...");
            Console.ReadKey();

            var currentMonitoringStepDto = new CurrentMonitoringStepDto() { BaseRefType = BaseRefType.Fast };
            signalRClient.NotifyMonitoringStep(currentMonitoringStepDto).Wait();

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
