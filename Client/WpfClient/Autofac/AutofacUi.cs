using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Serilog;

namespace Iit.Fibertest.Client
{
    public sealed class AutofacUi : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<UserListViewModel>();
            builder.RegisterType<ZonesViewModel>();
            builder.RegisterType<ObjectsToZonesViewModel>();
            builder.RegisterType<ZonesContentViewModel>();

            var logger = new LoggerConfiguration()
                .WriteTo.Seq(@"http://localhost:5341").CreateLogger();
            builder.RegisterInstance<ILogger>(logger);
            logger.Information("");
            logger.Information(new string('-', 99));

            var iniFile = new IniFile();
            iniFile.AssignFile(@"Client.ini");
            builder.RegisterInstance(iniFile);

            var culture = iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            var logFileLimitKb = iniFile.Read(IniSection.General, IniKey.LogFileSizeLimitKb, 0);

            var logFile = new LogFile();
            logFile.AssignFile(@"Client.log", logFileLimitKb, culture); // this couldn't be done in ctor becauses of tests using shellVM's ctor
            logFile.EmptyLine();
            logFile.EmptyLine('-');
            builder.RegisterInstance(logFile);
        }


    }


}