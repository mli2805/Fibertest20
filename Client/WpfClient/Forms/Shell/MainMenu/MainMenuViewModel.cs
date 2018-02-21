using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private CurrentUser _currentUser;

        private bool _isZonesEnabled;
        public bool IsZonesEnabled
        {
            get => _isZonesEnabled;
            set
            {
                if (value == _isZonesEnabled) return;
                _isZonesEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
        }

        public void Initialize(CurrentUser currentUser)
        {
            _currentUser = currentUser;
            IsZonesEnabled = _currentUser.Role <= Role.Root;
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
