using System.Globalization;
using System.Reflection;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.SuperClient
{
    using Autofac;
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;

    public class AppBootstrapper : BootstrapperBase
    {
       // SimpleContainer container;
        private ILifetimeScope _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            //            container = new SimpleContainer();
            //
            //            container.Singleton<IWindowManager, WindowManager>();
            //            container.Singleton<IEventAggregator, EventAggregator>();
            //            container.PerRequest<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            //            return container.GetInstance(service, key);
            return string.IsNullOrWhiteSpace(key) ?
                _container.Resolve(service) :
                _container.ResolveNamed(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
//            return container.GetAllInstances(service);
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
//            container.BuildUp(instance);
            _container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacSuperClient>();

            var iniFile = new IniFile();
            var iniFileName = @"sc.ini";
            iniFile.AssignFile(iniFileName);
            builder.RegisterInstance(iniFile); 
            
            _container = builder.Build();

            var currentCulture = iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

            DisplayRootViewFor<IShell>();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(ShellView).Assembly; // this Assembly (.exe)
            yield return typeof(WpfCommonViews.RftsEventsView).Assembly; // WpfCommonViews
        }
    }
}