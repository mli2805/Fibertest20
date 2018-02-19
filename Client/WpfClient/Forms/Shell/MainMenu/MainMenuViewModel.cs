using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel
    {
        private readonly IWindowManager _windowManager;
        private readonly UserListViewModel _userListViewModel;
        private readonly ZonesViewModel _zonesViewModel;

        public MainMenuViewModel(IWindowManager windowManager, UserListViewModel userListViewModel, 
            ZonesViewModel zonesViewModel)
        {
            _windowManager = windowManager;
            _userListViewModel = userListViewModel;
            _zonesViewModel = zonesViewModel;
        }

        public async void LaunchUserListView()
        {
            await _userListViewModel.Initialize();
            _windowManager.ShowWindowWithAssignedOwner(_userListViewModel);
        }

        public async void LaunchResponsibilityZonesView()
        {
            await _zonesViewModel.Initialize();
            _windowManager.ShowWindowWithAssignedOwner(_zonesViewModel);
        }

        public void LaunchObjectsToZonesView()
        {
        }
    }
}
