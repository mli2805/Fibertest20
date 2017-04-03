using System;
using System.IO;
using System.Runtime.InteropServices;
using Autofac;
using Caliburn.Micro;
using Serilog;

namespace Iit.Fibertest.TestBench
{
    public sealed class AutofacUi : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<UserListViewModel>();
            builder.RegisterType<ZonesContentViewModel>();

//            builder.RegisterInstance<ILogger>(
//                new LoggerConfiguration()
//                    .WriteTo.RollingFile("logs\\client.log")
//                    .CreateLogger());

            var logger = new LoggerConfiguration()
                .WriteTo.Seq(@"http://localhost:5341")
                .CreateLogger();

            builder.RegisterInstance<ILogger>(logger);

            builder.RegisterInstance(new IniFile(IniFileName(@"client.ini", logger)));
        }

        private string IniFileName(string filename, ILogger log)
        {
            try
            {
                string iniFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\ini\"));
                if (!Directory.Exists(iniFolder))
                    Directory.CreateDirectory(iniFolder);

                var iniFileName = Path.GetFullPath(Path.Combine(iniFolder, filename));
                if (!File.Exists(iniFileName))
                   File.Create(iniFileName);
                return iniFileName;
            }
            catch (COMException e)
            {
                log.Information(e.Message);
                return null;
            }
        }
    }

  
}