using System.ServiceProcess;
using Autofac;
using Iit.Fibertest.UtilsLib;

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

    public static class AutofacForRtuExtensions
    {
        public static ContainerBuilder ForRtu(this ContainerBuilder builder)
        {
            var serviceIni = new IniFile().AssignFile("RtuService.ini");
            builder.RegisterInstance(serviceIni);

            var serviceLog = new LogFile(serviceIni);
            builder.RegisterInstance<IMyLog>(serviceLog);

            builder.RegisterType<Service1>().As<ServiceBase>().SingleInstance();

            return builder;
        }
    }
}
