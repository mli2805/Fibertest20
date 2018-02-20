using Autofac;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly UserListViewModel _userListViewModel;

        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager, UserListViewModel userListViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _userListViewModel = userListViewModel;
        }

        public void LaunchResponsibilityZonesView()
        {
            var vm = _globalScope.Resolve<ZonesViewModel>();
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public async void LaunchUserListView()
        {
            await _userListViewModel.Initialize();
            _windowManager.ShowWindowWithAssignedOwner(_userListViewModel);
        }

        public void LaunchObjectsToZonesView()
        {
        }
    }
}
