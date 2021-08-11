using System.Globalization;
using System.ServiceProcess;
using System.Threading;
using Autofac;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterTrapReceiver
{
    public static class AutofacTrapReceiver
    {
        public static ContainerBuilder WithProduction(this ContainerBuilder builder)
        {
            var iniFile = new IniFile().AssignFile("TrapReceiver.ini");
            builder.RegisterInstance(iniFile);
            var currentCulture = iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

            var logFile = new LogFile(iniFile, 100000);
            builder.RegisterInstance<IMyLog>(logFile);
           
            builder.RegisterType<Service1>().As<ServiceBase>().SingleInstance();

            return builder;
        }
    }
}
