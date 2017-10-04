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

            ServiceBase.Run(container.Resolve<ServiceBase>());

//            ServiceBase[] ServicesToRun;
//            ServicesToRun = new ServiceBase[]
//            {
//                new Service1()
//            };
//            ServiceBase.Run(ServicesToRun);
        }
    }
}
