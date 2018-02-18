using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel
    {
        private readonly IWindowManager _windowManager;
        private readonly UserListViewModel _userListViewModel;
        private readonly ZonesViewModel _zonesViewModel;
        private readonly ZonesContentViewModel _zonesContentViewModel;

        public MainMenuViewModel(IWindowManager windowManager, UserListViewModel userListViewModel, 
            ZonesViewModel zonesViewModel, ZonesContentViewModel zonesContentViewModel)
        {
            _windowManager = windowManager;
            _userListViewModel = userListViewModel;
            _zonesViewModel = zonesViewModel;
            _zonesContentViewModel = zonesContentViewModel;
        }

        public async void LaunchUserListView()
        {
            await _userListViewModel.Initialize();
            _windowManager.ShowWindowWithAssignedOwner(_userListViewModel);
        }

        public void LaunchResponsibilityZonesView()
        {
            _windowManager.ShowWindowWithAssignedOwner(_zonesViewModel);
        }

        public void LaunchObjectsToZonesView()
        {
            _windowManager.ShowWindowWithAssignedOwner(_zonesContentViewModel);
        }
    }
}
