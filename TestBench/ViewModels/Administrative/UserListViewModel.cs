using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class UserListViewModel : Screen
    {
        private readonly UsersDb _usersDb;
        private readonly IWindowManager _windowManager;
        private UserVm _selectedUserVm;
        public ObservableCollection<UserVm> Rows { get; set; }

        public UserVm SelectedUserVm
        {
            get { return _selectedUserVm; }
            set
            {
                _selectedUserVm = value;
                NotifyOfPropertyChange(nameof(IsRemovable));
            }
        }

        public bool IsRemovable => SelectedUserVm?.Role != Role.Root;

        public static List<Role> Roles { get; set; }

        public UserListViewModel(UsersDb usersDb, IWindowManager windowManager)
        {
            _usersDb = usersDb;
            _windowManager = windowManager;
            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();

            MapUserList();
        }

        private void MapUserList()
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserMappings>()).CreateMapper();
            var intermediateList = _usersDb.Users.Select(user => mapper.Map<UserVm>(user)).ToList();
            intermediateList.ForEach(u => u.ZoneName =
                u.ZoneId != Guid.Empty
                    ? _usersDb.Zones.First(z => z.Id == u.ZoneId).Title
                    : u.IsDefaultZoneUser
                        ? Resources.SID_Default_Zone
                        : Resources.SID_No_zone_assigned);
            Rows = new ObservableCollection<UserVm>(intermediateList);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User_list;
        }

        #region One User Actions
        public void AddNewUser()
        {
            var userUnderConstruction = new UserVm();
            var vm = new UserViewModel(userUnderConstruction, _usersDb.Zones);
            if (_windowManager.ShowDialog(vm) == true)
            {
                Rows.Add(userUnderConstruction);
                SelectedUserVm = Rows.Last();
            }
        }

        public void ChangeUser()
        {
            var userUnderConstruction =  (UserVm)SelectedUserVm.Clone();
            var vm = new UserViewModel(userUnderConstruction, _usersDb.Zones);
            if (_windowManager.ShowDialog(vm) == true)
            {
                userUnderConstruction.CopyTo(SelectedUserVm);
            }
        }

        public void RemoveUser()
        {
            _usersDb.Users.Remove(_usersDb.Users.First(u => u.Id == SelectedUserVm.Id));
            Rows.Remove(SelectedUserVm);
            SelectedUserVm = Rows.First();
        }
        #endregion

        public void Save()
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserMappings>()).CreateMapper();
            _usersDb.Users = Rows.Select(userVm => mapper.Map<User>(userVm)).ToList();
            _usersDb.Save();
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
