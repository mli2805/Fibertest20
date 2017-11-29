using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel
    {
        private readonly IWindowManager _windowManager;

        public MainMenuViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void LaunchUserListView()
        {
            var vm = IoC.Get<UserListViewModel>();
            _windowManager.ShowWindow(vm);
        }

        public void LaunchResponsibilityZonesView()
        {
            var vm = IoC.Get<ZonesViewModel>();
            _windowManager.ShowWindow(vm);
        }

        public void LaunchObjectsToZonesView()
        {
            var vm = IoC.Get<ZonesContentViewModel>();
            _windowManager.ShowWindow(vm);
        }
    }
}
