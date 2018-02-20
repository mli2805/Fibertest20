using Autofac;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel
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
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void LaunchUserListView()
        {
            var vm = _globalScope.Resolve<UserListViewModel>();
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void LaunchObjectsToZonesView()
        {
        }
    }
}
