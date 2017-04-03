using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class UserListViewModel : Screen
    {
        private readonly UsersDb _usersDb;
        private readonly IWindowManager _windowManager;
        private User _selectedUser;
        public ObservableCollection<User> Rows { get; set; }

        public User SelectedUser
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

        public UserListViewModel(UsersDb usersDb, IWindowManager windowManager)
        {
            _usersDb = usersDb;
            _windowManager = windowManager;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            usersDb.Users.ForEach(u => u.ZoneName = 
                u.ZoneId != Guid.Empty 
                    ? usersDb.Zones.First(z=>z.Id == u.ZoneId).Name 
                    : u.IsDefaultZoneUser 
                            ? Resources.SID_Default_Zone 
                            : Resources.SID_No_zone_assigned);
            Rows = new ObservableCollection<User>(usersDb.Users);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User_list;
        }

        public void AddNewUser()
        {
            var userUnderConstruction = new User();
            var vm = new UserViewModel(true, userUnderConstruction, _usersDb.Zones);
            if (_windowManager.ShowDialog(vm) == true)
            {
                _usersDb.Users.Add(userUnderConstruction);
                Rows.Add(userUnderConstruction);
                SelectedUser = Rows.Last();
            }
        }

        public void ChangeUser()
        {
            var userUnderConstruction =  (User)SelectedUser.Clone();
            var vm = new UserViewModel(false, userUnderConstruction, _usersDb.Zones);
            if (_windowManager.ShowDialog(vm) == true)
            {
                userUnderConstruction.CopyTo(SelectedUser);
            }
        }

        public void RemoveUser()
        {
            _usersDb.Users.Remove(_usersDb.Users.First(u => u.Id == SelectedUser.Id));
            Rows.Remove(SelectedUser);
            SelectedUser = Rows.First();
        }

        public void Close()
        {
            TryClose();
        }
    }
}
