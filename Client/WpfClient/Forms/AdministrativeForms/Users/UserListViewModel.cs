using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class UserListViewModel : Screen
    {
        private List<User> _users;
        private List<Zone> _zones;
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly EventArrivalNotifier _eventArrivalNotifier;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly CurrentUser _currentUser;

        private ObservableCollection<UserVm> _rows = new ObservableCollection<UserVm>();
        public ObservableCollection<UserVm> Rows
        {
            get => _rows;
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        private UserVm _selectedUser;
        public UserVm SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                NotifyOfPropertyChange(nameof(CanEdit));
                NotifyOfPropertyChange(nameof(CanRemove));
            }
        }

        public static List<Role> Roles { get; set; }
        public bool CanAdd => _currentUser.Role <= Role.Root;
        public bool CanEdit => _currentUser.Role <= Role.Root || _currentUser.UserId == SelectedUser?.UserId;
        public bool CanRemove => _currentUser.Role <= Role.Root && SelectedUser?.Role != Role.Root;

        public UserListViewModel(ILifetimeScope globalScope, Model readModel, EventArrivalNotifier eventArrivalNotifier,
            IWindowManager windowManager, IWcfServiceDesktopC2D c2DWcfManager, CurrentUser currentUser)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _eventArrivalNotifier = eventArrivalNotifier;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _currentUser = currentUser;

            Initialize();
        }

        private void Initialize()
        {
            _users = _readModel.Users;
            _zones = _readModel.Zones;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            foreach (var user in _users.Where(u => u.Role >= _currentUser.Role ))
                Rows.Add(new UserVm(user, _zones.First(z => z.ZoneId == user.ZoneId).Title));

            _eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
            SelectedUser = Rows.First();
        }

        private void _eventArrivalNotifier_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Rows = new ObservableCollection<UserVm>();
            foreach (var user in _users.Where(u => u.Role >= _currentUser.Role))
                Rows.Add(new UserVm(user, _zones.First(z => z.ZoneId == user.ZoneId).Title));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User_list;
        }

        #region One User Actions
        public void AddNewUser()
        {
            var vm = _globalScope.Resolve<UserViewModel>();
            vm.InitializeForCreate();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void ChangeUser()
        {
            var userInWork = (UserVm)SelectedUser.Clone();
            var vm = _globalScope.Resolve<UserViewModel>();
            vm.InitializeForUpdate(userInWork);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void RemoveUser()
        {
            var cmd = new RemoveUser() { UserId = SelectedUser.UserId };
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }
        #endregion

        public void Close()
        {
            TryClose();
        }
    }
}
