using System.Globalization;
using System.Reflection;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace LicenseViewer {
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;

    public class AppBootstrapper : BootstrapperBase {
        SimpleContainer _container;

        public AppBootstrapper() {
            Initialize();
        }

        protected override void Configure() {
            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key) {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance) {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) 
        {
            var iniFile = new IniFile();
            iniFile.AssignFile("licviewer.ini");
            var currentCulture = iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

            DisplayRootViewFor<IShell>();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(ShellView).Assembly; // this Assembly (.exe)
            yield return typeof(Iit.Fibertest.WpfCommonViews.RftsEventsView).Assembly; // WpfCommonViews
        }
    }
}