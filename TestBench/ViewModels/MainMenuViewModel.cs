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

        public void LaunchUsersListView()
        {
            var vm = new UserListViewModel(_windowManager);
            _windowManager.ShowDialog(vm);
        }

        public void LaunchResponsibilityZonesView()
        {
            
        }
    }
}
