using Autofac;
using System.Windows;
using Iit.Fibertest.Setup;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Setup
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using Caliburn.Micro;

    public class AppBootstrapper : BootstrapperBase
    {
        // SimpleContainer container;

        private ILifetimeScope _container;
        private CurrentInstallation _currentInstallation;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            //            container = new SimpleContainer();

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
            builder.RegisterModule<AutofacInSetup>();

            var iniFile = new IniFile();
            iniFile.AssignFile("setup.ini");
            builder.RegisterInstance(iniFile);

            _container = builder.Build();

            _currentInstallation = _container.Resolve<CurrentInstallation>();
            _currentInstallation.ProductName = "IIT Fibertest";
           
            SetCurrentCulture();


            DisplayRootViewFor<IShell>();
        }

        private void SetCurrentCulture()
        {
            var culture = RegistryOperations.GetPreviousInstallationCulture();
            if (string.IsNullOrEmpty(culture))
                AskAndSetCulture();
            else
                SetCultureAskContinuation(culture);
        }

        private void SetCultureAskContinuation(string culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            var result = MessageBox.Show(
                string.Format(Resources.SID__0__installed_on_your_PC_already__Continue_, _currentInstallation.MainName),
                Resources.SID_Confirmation, MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void AskAndSetCulture()
        {
            ((Iit.Fibertest.Setup.App)Application.Current).ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var vm = _container.Resolve<InstallationLanguageViewModel>();
            var wm = _container.Resolve<IWindowManager>();
            wm.ShowDialog(vm);
            if (!vm.IsOkPressed)
            {
                Application.Current.Shutdown();
                return;
            }
            var culture = (vm.SelectedLanguage == "English") ? "en-US" : "ru-RU";
            RegistryOperations.SaveSetupCultureInRegistry(culture);

            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            ((Iit.Fibertest.Setup.App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}