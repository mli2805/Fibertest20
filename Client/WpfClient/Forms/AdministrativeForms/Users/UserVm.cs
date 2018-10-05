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
        private string _zoneTitle;

        public Guid UserId { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public Role Role
        {
            get => _role;
            set
            {
                if (value == _role) return;
                _role = value;
                NotifyOfPropertyChange();
            }
        }

        public string Password { get; set; } = "";

        public string EmailAddress
        {
            get => _email;
            set
            {
                if (value == _email) return;
                _email = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsEmailActivated
        {
            get => _isEmailActivated;
            set
            {
                if (value == _isEmailActivated) return;
                _isEmailActivated = value;
                NotifyOfPropertyChange();
            }
        }

        public SmsReceiverViewModel SmsReceiverVm { get; set; }

        public Guid ZoneId
        {
            get => _zoneId;
            set
            {
                if (value.Equals(_zoneId)) return;
                _zoneId = value;
                NotifyOfPropertyChange();
            }
        }

        public string ZoneTitle
        {
            get => _zoneTitle;
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

            EmailAddress = user.Email.Address;
            IsEmailActivated = user.Email.IsActivated;

            SmsReceiverVm = new SmsReceiverViewModel()
            {
                PhoneNumber = user.Sms.PhoneNumber,
                IsFiberBreakOn = user.Sms.IsFiberBreakOn,
                IsCriticalOn = user.Sms.IsCriticalOn,
                IsMajorOn = user.Sms.IsMajorOn,
                IsMinorOn = user.Sms.IsMinorOn,
                IsOkOn = user.Sms.IsOkOn,
                IsNetworkEventsOn = user.Sms.IsNetworkEventsOn,
                IsBopEventsOn = user.Sms.IsBopEventsOn,
                IsActivated = user.Sms.IsActivated,
            };

            ZoneTitle = zoneTitle;
            ZoneId = user.ZoneId;
        }

        public object Clone()
        {
            var userVm = (UserVm)MemberwiseClone();
            userVm.SmsReceiverVm = new SmsReceiverViewModel()
            {
                PhoneNumber = SmsReceiverVm.PhoneNumber,
                IsFiberBreakOn = SmsReceiverVm.IsFiberBreakOn,
                IsCriticalOn = SmsReceiverVm.IsCriticalOn,
                IsMajorOn = SmsReceiverVm.IsMajorOn,
                IsMinorOn = SmsReceiverVm.IsMinorOn,
                IsOkOn = SmsReceiverVm.IsOkOn,
                IsNetworkEventsOn = SmsReceiverVm.IsNetworkEventsOn,
                IsBopEventsOn = SmsReceiverVm.IsBopEventsOn,
                IsActivated = SmsReceiverVm.IsActivated,
            };
            return userVm;
        }


    }
}