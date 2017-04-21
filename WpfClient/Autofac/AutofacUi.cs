using System;
using System.IO;
using System.Runtime.InteropServices;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Utils35;
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

            builder.RegisterInstance(new IniFile(FileNameForSure(@"..\ini\", @"client.ini", logger)));
            builder.RegisterInstance(new Logger35());
        }


        private string FileNameForSure(string foldername, string filename, ILogger log)
        {
            try
            {
                string folder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, foldername));
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var result = Path.GetFullPath(Path.Combine(folder, filename));
                if (!File.Exists(result))
                   File.Create(result);
                return result;
            }
            catch (COMException e)
            {
                log.Information(e.Message);
                return null;
            }
        }
    }

  
}