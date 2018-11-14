using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class AppBootstrapper : BootstrapperBase
    {
        private ILifetimeScope _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
        }


        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ?
                _container.Resolve(service) :
                _container.ResolveNamed(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            SomeInitialActions(e.Args);

            DisplayRootViewFor<IShell>();
        }

        private void SomeInitialActions(string[] commandLineParams)
        {
            var postfix = commandLineParams.Length == 0 ? "" : commandLineParams[0];

            String thisprocessname = Process.GetCurrentProcess().ProcessName;
            if ((postfix == "") && (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1))
                Environment.FailFast(@"Fast termination of application.");

            var iniFileName = $@"client{postfix}.ini";

            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacClient>();

            var iniFile = new IniFile();
            iniFile.AssignFile(iniFileName);
            builder.RegisterInstance(iniFile);

            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            iniFile.Write(IniSection.General, IniKey.Version, info.FileVersion);
     
            var parameters = ParseCommandLine(commandLineParams);
            builder.RegisterInstance(parameters);
            _container = builder.Build();

            var currentCulture = postfix == "" ? iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU") : parameters.SuperClientCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

            // Ensure the current culture passed into bindings 
            // is the OS culture. By default, WPF uses en-US 
            // as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        private CommandLineParameters ParseCommandLine(string[] args)
        {
            var parameters = new CommandLineParameters();
            parameters.IsUnderSuperClientStart = args.Length != 0;
            if (parameters.IsUnderSuperClientStart)
            {
                if (int.TryParse(args[0], out int clientOrdinal))
                    parameters.ClientOrdinal = clientOrdinal;
                parameters.SuperClientCulture = args[1];
                if (args.Length >= 4)
                {
                    parameters.Username = args[2];
                    parameters.Password = args[3];
                }

                if (args.Length == 6)
                {
                    if (int.TryParse(args[5], out int serverPort))
                        parameters.ServerNetAddress = new NetAddress(args[4], serverPort);
                }
            }
            return parameters;
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(ShellView).Assembly; // this Assembly (.exe)
            yield return typeof(WpfCommonViews.RftsEventsView).Assembly; // WpfCommonViews
        }
    }
}