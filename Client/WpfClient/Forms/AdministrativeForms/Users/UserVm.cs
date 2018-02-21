using System;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class UserVm : PropertyChangedBase, ICloneable
    {
        private string _title;
        private Role _role;
        private string _email;
        private bool _isEmailActivated;
        private Guid _zoneId;
        private bool _isDefaultZoneUser;
        private string _zoneTitle;

        public Guid UserId { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
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

        public string ZoneTitle
        {
            get { return _zoneTitle; }
            set
            {
                if (value == _zoneTitle) return;
                _zoneTitle = value;
                NotifyOfPropertyChange();
            }
        }

        public UserVm()
        {
            UserId = Guid.NewGuid();
            Role = Role.Operator;
        }
        public UserVm(User user, string zoneTitle)
        {
            UserId = user.UserId;
            Title = user.Title;
            Password = UserExt.FlipFlop(user.EncodedPassword);
            Role = user.Role;
            Email = user.Email;
            IsEmailActivated = user.IsEmailActivated;
            IsDefaultZoneUser = user.IsDefaultZoneUser;
            ZoneTitle = zoneTitle;
            ZoneId = user.ZoneId;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void CopyTo(UserVm destination)
        {
            destination.UserId = UserId;
            destination.Title = Title;
            destination.Role = Role;
            destination.Password = Password;
            destination.Email = Email;
            destination.IsEmailActivated = IsEmailActivated;
            destination.ZoneId = ZoneId;
            destination.IsDefaultZoneUser = IsDefaultZoneUser;
            destination.ZoneTitle = ZoneTitle;
        }
    }
}