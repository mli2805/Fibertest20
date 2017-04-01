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
            var vm = new ZonesViewModel();
            _windowManager.ShowDialog(vm);
        }
    }
}
