using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class UserListViewModel : Screen
    {
        private List<User> _users;
        private List<Zone> _zones;
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        public ObservableCollection<User> Rows { get; set; }

        private UserVm _selectedUser;
        public UserVm SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                NotifyOfPropertyChange(nameof(IsRemovable));
            }
        }

        public bool IsRemovable => SelectedUser?.Role != Role.Root;

        public static List<Role> Roles { get; set; }

        public UserListViewModel(ILifetimeScope globalScope, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
        }

        public async Task<int> Initialize()
        {
            _users = await _c2DWcfManager.GetUsersAsync();
            _zones = await _c2DWcfManager.GetZonesAsync();
            var defaultZone = _zones.First(z => z.IsDefaultZone);
            defaultZone.Title = Resources.SID_Default_Zone;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            Rows = new ObservableCollection<User>(_users);

            return 1;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User_list;
        }

        #region One User Actions
        public void AddNewUser()
        {
            var vm = _globalScope.Resolve<UserViewModel>();
            vm.Initialize(new User(), _zones);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) == true)
            {
                Rows.Add(vm.UserInWork);
            }
        }

        public void ChangeUser()
        {
            var userInWork =  (User)SelectedUser.Clone();
            var vm = _globalScope.Resolve<UserViewModel>();
            vm.Initialize(userInWork, _zones);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) == true)
            {
                var oldUser = Rows.First(u => u.Id == userInWork.Id);
                Rows.Remove(oldUser);
                Rows.Add(vm.UserInWork);
            }
        }

        public void RemoveUser()
        {
          
        }
        #endregion

        public void Close()
        {
            TryClose();
        }

       
    }
}
