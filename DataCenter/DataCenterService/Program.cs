using System;
using System.ServiceProcess;
using Autofac;

namespace Iit.Fibertest.DataCenterService
{
  
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
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
