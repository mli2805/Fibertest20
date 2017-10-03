using System;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Serilog;
using WcfConnections;

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
            builder.RegisterType<ClientWcfServiceHost>().As<IClientWcfServiceHost>();

            var logger = new LoggerConfiguration()
                .WriteTo.Seq(@"http://localhost:5341").CreateLogger();
            builder.RegisterInstance<ILogger>(logger);
            logger.Information("");
            logger.Information(new string('-', 99));

            var iniFile = new IniFile();
            iniFile.AssignFile(@"Client.ini");
            builder.RegisterInstance(iniFile);


            
            builder.Register<IMyLog>(ctx =>
            {
                var logFile = new LogFile(ctx.Resolve<IniFile>()).AssignFile(@"Client.log");
                logFile.EmptyLine();
                logFile.EmptyLine('-');
                return logFile;
            }).SingleInstance();

            var clientId = Guid.Parse(iniFile.Read(IniSection.General, IniKey.ClientGuidOnServer, Guid.NewGuid().ToString()));
            var serverAddresses = iniFile.ReadDoubleAddress(11840);
            builder.Register(ctx => new C2DWcfManager(
                serverAddresses, iniFile, ctx.Resolve<IMyLog>(), clientId));
        }
    }
}