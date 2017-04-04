using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
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
            _windowManager.ShowDialog(vm);
        }

        public void LaunchResponsibilityZonesView()
        {
            var vm = IoC.Get<ZonesViewModel>();
            _windowManager.ShowDialog(vm);
        }

        public void LaunchObjectsToZonesView()
        {
            var vm = IoC.Get<ZonesContentViewModel>();
            _windowManager.ShowDialog(vm);
        }
    }
}
