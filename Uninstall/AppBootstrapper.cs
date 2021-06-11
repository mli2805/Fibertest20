using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Uninstall
{
    public class AppBootstrapper : BootstrapperBase
    {
        SimpleContainer _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            SetCurrentCulture();

            DisplayRootViewFor<IShell>();
        }

        private void SetCurrentCulture()
        {
            var culture = RegistryOperations.GetPreviousInstallationCulture();
            if (string.IsNullOrEmpty(culture))
                culture = "en-US";

            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }

    }
}