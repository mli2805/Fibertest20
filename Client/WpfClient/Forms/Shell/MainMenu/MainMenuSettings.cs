using Autofac;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel
    {
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
        public void LaunchSnmpSettingsView()
        {
            var vm = _globalScope.Resolve<SnmpSettingsViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchClientSettingsView()
        {
            var vm = _globalScope.Resolve<ConfigurationViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchGisSettingsView()
        {
            var vm = _globalScope.Resolve<GisSettingsViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchCleaningView()
        {
            var vm = _globalScope.Resolve<DbOptimizationViewModel>();
            vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}