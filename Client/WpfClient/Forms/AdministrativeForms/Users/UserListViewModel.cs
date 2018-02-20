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
        private readonly ILifetimeScope _globalScope;
        private readonly ReadModel _readModel;
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

        public UserListViewModel(ILifetimeScope globalScope, ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
        }

        public void Initialize()
        {
            _users = _readModel.Users;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            Rows = new ObservableCollection<User>(_users);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User_list;
        }

        #region One User Actions
        public void AddNewUser()
        {
            var vm = _globalScope.Resolve<UserViewModel>();
            vm.Initialize(new User());
            if (_windowManager.ShowDialogWithAssignedOwner(vm) == true)
            {
                Rows.Add(vm.UserInWork);
            }
        }

        public void ChangeUser()
        {
            var userInWork =  (User)SelectedUser.Clone();
            var vm = _globalScope.Resolve<UserViewModel>();
            vm.Initialize(userInWork);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) == true)
            {
                var oldUser = Rows.First(u => u.UserId == userInWork.UserId);
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
