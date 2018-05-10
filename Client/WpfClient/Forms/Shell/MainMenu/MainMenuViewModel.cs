using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly Model _readModel;
        private CurrentUser _currentUser;

      
        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager, Model readModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _readModel = readModel;
        }

        public void Initialize(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

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

        public void LaunchChangePasswordView()
        {
            var vm = _globalScope.Resolve<ChangePasswordViewModel>();
            var user = _readModel.Users.First(u => u.Title == _currentUser.UserName);
            vm.Initialize(user);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchConfigurationView()
        {
            var vm = _globalScope.Resolve<ConfigurationViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}
