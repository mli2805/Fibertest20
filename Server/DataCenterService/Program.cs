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
            ServiceBase.Run(container.Resolve<ServiceBase[]>());
        }
    }
}
