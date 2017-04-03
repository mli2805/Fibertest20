using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class UserViewModel : Screen
    {
        public User User { get; set; }
        private readonly bool _isCreateNewUserMode;

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

        public UserViewModel(bool isCreateNewUserMode, User user, List<Zone> zones)
        {
            User = user;
            _isCreateNewUserMode = isCreateNewUserMode;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Skip(2).ToList();
            User.Role = Roles.First();

            Zones = zones;
            SelectedZone = Zones.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User;
        }

        public void Save()
        {
            User.ZoneId = SelectedZone.Id;
            User.ZoneName = SelectedZone.Name;
            User.IsDefaultZoneUser = SelectedZone.Name == Resources.SID_Default_Zone;

            TryClose(true);
        }
        public void Cancel()
        {

            TryClose(false);
        }
    }
}
