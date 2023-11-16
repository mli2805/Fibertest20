using Autofac;
using System.Windows;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.InstallRtu
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using Caliburn.Micro;

    public class AppBootstrapper : BootstrapperBase
    {
        private ILifetimeScope _container;
        private CurrentRtuInstallation _currentRtuInstallation;

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
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacInRtuInstall>();

            var iniFile = new IniFile();
            iniFile.AssignFile("setup.ini");
            builder.RegisterInstance(iniFile);

            _container = builder.Build();

            _currentRtuInstallation = _container.Resolve<CurrentRtuInstallation>();
            _currentRtuInstallation.ProductName = "IIT Fibertest";
            _currentRtuInstallation.Args = e.Args;

            // in Visual Studio
            if (e.Args.Length == 1 && e.Args[0].ToLower() == @"/admin")
            {
                _currentRtuInstallation.IsAdmin = true;
            }
            // in real life
            if (e.Args.Length == 2 && e.Args[1].ToLower() == @"/admin")
            {
                _currentRtuInstallation.IsAdmin = true;
            }

            if (_currentRtuInstallation.IsAdmin)
                SetCurrentCulture();
            else
                SetEnUsCulture();
            DisplayRootViewFor<IShell>();
        }

        private void SetEnUsCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
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
                string.Format(Resources.SID__0__installed_on_your_PC_already__Continue_, _currentRtuInstallation.MainName),
                Resources.SID_Confirmation, MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void AskAndSetCulture()
        {
            ((App)Application.Current).ShutdownMode = ShutdownMode.OnExplicitShutdown;

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

            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}