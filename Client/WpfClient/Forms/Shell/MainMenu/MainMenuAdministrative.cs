using Autofac;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel
    {
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

    }
}
