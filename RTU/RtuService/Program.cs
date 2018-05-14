using System;
using System.ServiceProcess;
using Autofac;

namespace Iit.Fibertest.RtuService
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var builder = new ContainerBuilder().ForRtu();
            var container = builder.Build();

            if (Environment.UserInteractive)
            {
                Service1 service1 = (Service1)container.Resolve<ServiceBase>();
                service1.TestStartupAndStop(null);
            }
            else
            {
                ServiceBase.Run(container.Resolve<ServiceBase>());
            }
        }
    }
}
