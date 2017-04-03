using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class UserViewModel : Screen
    {
        public UserVm UserVm { get; set; }

        public static List<Role> Roles { get; set; }
        public static List<Zone> Zones { get; set; }

        private Zone _selectedZone;
        public Zone SelectedZone
        {
            get { return _selectedZone; }
            set
            {
                if (Equals(value, _selectedZone)) return;
                _selectedZone = value;
                NotifyOfPropertyChange();
            }
        }

        public UserViewModel(UserVm userVm, List<Zone> zones)
        {
            UserVm = userVm;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Skip(2).ToList();
            if (UserVm.Role == 0)
                UserVm.Role = Roles.First();

            Zones = zones;
            SelectedZone = Zones.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User;
        }

        public void Save()
        {
            UserVm.ZoneId = SelectedZone.Id;
            UserVm.ZoneName = SelectedZone.Title;
            UserVm.IsDefaultZoneUser = SelectedZone.Title == Resources.SID_Default_Zone;

            TryClose(true);
        }
        public void Cancel()
        {
            TryClose(false);
        }
    }
}
