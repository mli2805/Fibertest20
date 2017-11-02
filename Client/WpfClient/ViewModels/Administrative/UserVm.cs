using System;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class UserVm : PropertyChangedBase, ICloneable
    {
        private string _name;
        private Role _role;
        private string _email;
        private bool _isEmailActivated;
        private string _zoneName;
        private Guid _zoneId;
        private bool _isDefaultZoneUser;
        public Guid Id { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyOfPropertyChange();
            }
        }

        public Role Role
        {
            get { return _role; }
            set
            {
                if (value == _role) return;
                _role = value;
                NotifyOfPropertyChange();
            }
        }

        public string Password { get; set; } = "";

        public string Email
        {
            get { return _email; }
            set
            {
                if (value == _email) return;
                _email = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsEmailActivated
        {
            get { return _isEmailActivated; }
            set
            {
                if (value == _isEmailActivated) return;
                _isEmailActivated = value;
                NotifyOfPropertyChange();
            }
        }

        public Guid ZoneId
        {
            get { return _zoneId; }
            set
            {
                if (value.Equals(_zoneId)) return;
                _zoneId = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsDefaultZoneUser
        {
            get { return _isDefaultZoneUser; }
            set
            {
                if (value == _isDefaultZoneUser) return;
                _isDefaultZoneUser = value;
                NotifyOfPropertyChange();
            }
        }

        public string ZoneName
        {
            get { return _zoneName; }
            set
            {
                if (value == _zoneName) return;
                _zoneName = value;
                NotifyOfPropertyChange();
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void CopyTo(UserVm destination)
        {
            destination.Id = Id;
            destination.Name = Name;
            destination.Role = Role;
            destination.Password = Password;
            destination.Email = Email;
            destination.IsEmailActivated = IsEmailActivated;
            destination.ZoneId = ZoneId;
            destination.IsDefaultZoneUser = IsDefaultZoneUser;
            destination.ZoneName = ZoneName;
        }
    }
}