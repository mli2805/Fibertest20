using System;
using System.ServiceProcess;
using Autofac;

namespace Iit.Fibertest.DataCenterTrapReceiver
{
    public class Program
    {
        static void Main()
        {
            var builder = new ContainerBuilder().WithProduction();
            var container = builder.Build();

            if (Environment.UserInteractive) // under VS
            {
                Service1 service1 = (Service1)container.Resolve<ServiceBase>();
                service1.TestStartupAndStop(null);
            }
            else
            {
                ServiceBase.Run(container.Resolve<ServiceBase[]>());
            }
        }
    }
}
