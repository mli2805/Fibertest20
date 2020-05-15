using System;
using SignalRClientLib;

namespace SignalRExperiment
{
    class Program
    {
        static void Main()
        {
            var signalRClient = new SignalRClient();
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
