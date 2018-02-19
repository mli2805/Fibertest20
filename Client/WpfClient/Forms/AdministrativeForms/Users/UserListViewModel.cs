using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        public ObservableCollection<UserVm> Rows { get; set; }

        private UserVm _selectedUserVm;
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

        public UserListViewModel(IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;

        }

        public async Task<int> Initialize()
        {
            _users = await _c2DWcfManager.GetUsersAsync();
            // TODO get zones from Db
            _zones = new List<Zone> {new Zone() {Id = Guid.Empty, Title = Resources.SID_Default_Zone, }};

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            MapUserVmList();

            return 1;
        }
        private void MapUserVmList()
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserMappings>()).CreateMapper();
            var intermediateList = _users.Where(u=>u.Role > Role.Developer).Select(user => mapper.Map<UserVm>(user)).ToList();
            intermediateList.ForEach(u => u.ZoneName =
                u.ZoneId != Guid.Empty
                    ? _zones.First(z => z.Id == u.ZoneId).Title
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
            var vm = new UserViewModel(userUnderConstruction, _zones);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) == true)
            {
                Rows.Add(userUnderConstruction);
                SelectedUserVm = Rows.Last(); // doesn't work
            }
        }

        public void ChangeUser()
        {
            var userUnderConstruction =  (UserVm)SelectedUserVm.Clone();
            var vm = new UserViewModel(userUnderConstruction, _zones);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) == true)
            {
                userUnderConstruction.CopyTo(SelectedUserVm);
            }
        }

        public void RemoveUser()
        {
            _users.Remove(_users.First(u => u.Id == SelectedUserVm.Id));
            Rows.Remove(SelectedUserVm);
            SelectedUserVm = Rows.First(); // doesn't work
        }
        #endregion

        public void Close()
        {
            TryClose();
        }

       
    }
}
