using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class UserListViewModel : Screen
    {
        private List<User> _users;
        private List<Zone> _zones;
        private readonly ILifetimeScope _globalScope;
        private readonly ReadModel _readModel;
        private readonly EventArrivalNotifier _eventArrivalNotifier;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;

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
                NotifyOfPropertyChange(nameof(IsRemovable));
            }
        }

        public bool IsRemovable => SelectedUser?.Role != Role.Root;

        public static List<Role> Roles { get; set; }

        public UserListViewModel(ILifetimeScope globalScope, ReadModel readModel, EventArrivalNotifier eventArrivalNotifier,
            IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _eventArrivalNotifier = eventArrivalNotifier;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;

            Initialize();
        }

        private void Initialize()
        {
            _users = _readModel.Users;
            _zones = _readModel.Zones;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            foreach (var user in _users.Where(u=>u.Role > Role.Developer))
                Rows.Add(new UserVm(user, _zones.First(z=>z.ZoneId == user.ZoneId).Title));

            _eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
            SelectedUser = Rows.First();
        }

        private void _eventArrivalNotifier_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Rows = new ObservableCollection<UserVm>();
            foreach (var user in _users.Where(u => u.Role > Role.Developer))
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
            var cmd = new RemoveUser(){UserId = SelectedUser.UserId};
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }
        #endregion

        public void Close()
        {
            TryClose();
        }
    }
}
