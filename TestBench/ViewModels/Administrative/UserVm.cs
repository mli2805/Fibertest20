using System;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class UserVm : PropertyChangedBase, ICloneable
    {
        private string _name;
        private Role _role;
        private string _email;
        private bool _isEmailActivated;
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

        public Guid ZoneId { get; set; }
        public bool IsDefaultZoneUser { get; set; }
        public string ZoneName { get; set; }

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