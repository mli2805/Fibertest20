using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;


        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
        }

        public void LaunchResponsibilityZonesView()
        {
            var vm = _globalScope.Resolve<ZonesViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchUserListView()
        {
            var vm = _globalScope.Resolve<UserListViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchObjectsToZonesView()
        {
            var vm = _globalScope.Resolve<ObjectsAsTreeToZonesViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchSmtpSettingsView()
        {
            var vm = _globalScope.Resolve<SmtpSettingsViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchSmsSettingsView()
        {
            var vm = _globalScope.Resolve<SmsSettingsViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchClientSettingsView()
        {
            var vm = _globalScope.Resolve<ConfigurationViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchEventLogView()
        {
            var vm = _globalScope.Resolve<EventLogViewModel>();
            await vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchLicenseView()
        {
            var vm = _globalScope.Resolve<LicenseViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchAboutView()
        {
            var vm = _globalScope.Resolve<AboutViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}
