using System;
using Iit.Fibertest.FtSignalRClientLib;

namespace SignalRExperiment
{
    class Program
    {
        static void Main()
        {
            var signalRClient = new FtSignalRClient();
            signalRClient.Build();
            signalRClient.Connect().Wait();

            Console.WriteLine("Press any key...");
            Console.ReadKey();

            signalRClient.Send().Wait();

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
