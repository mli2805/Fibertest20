namespace Uninstall
{
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;
    using Iit.Fibertest.UtilsLib;
    using System.Threading;
    using System.Globalization;

    public class AppBootstrapper : BootstrapperBase
    {
        SimpleContainer container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
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