using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class UserViewModel : Screen, IDataErrorInfo
    {
        public UserVm UserVm { get; set; }

        public bool IsntItRoot { get; set; }

        private string _password1;
        public string Password1
        {
            get { return _password1; }
            set
            {
                if (value == _password1) return;
                _password1 = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Password2));
            }
        }

        private string _password2;
        public string Password2
        {
            get { return _password2; }
            set
            {
                if (value == _password2) return;
                _password2 = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Password1));
            }
        }


        public static List<Role> Roles { get; set; }
        public static List<WpfZone> Zones { get; set; }

        private WpfZone _selectedWpfZone;
        public WpfZone SelectedWpfZone
        {
            get { return _selectedWpfZone; }
            set
            {
                if (Equals(value, _selectedWpfZone)) return;
                _selectedWpfZone = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonSaveEnabled;
        public bool IsButtonSaveEnabled
        {
            get { return _isButtonSaveEnabled; }
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public UserViewModel(UserVm userVm, List<WpfZone> zones)
        {
            UserVm = userVm;

            if (UserVm.Role == Role.Root)
            {
                Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Skip(1).ToList();
                IsntItRoot = false;
            }
            else
            {
                Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Skip(2).ToList();
                IsntItRoot = true;
            }
            if (UserVm.Role == 0)
                UserVm.Role = Roles.First();
            Password1 = Password2 = UserVm.Password;

            Zones = zones;

            SelectedWpfZone = (UserVm.IsDefaultZoneUser) ? Zones.First() : Zones.First(z=>z.Id == userVm.ZoneId);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User;
        }

        public void Save()
        {
            UserVm.ZoneId = SelectedWpfZone.Id;
            UserVm.ZoneName = SelectedWpfZone.Title;
            UserVm.IsDefaultZoneUser = SelectedWpfZone.Title == Resources.SID_Default_Zone;
            UserVm.Password = Password1;

            TryClose(true);
        }
        public void Cancel()
        {
            TryClose(false);
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Password1":
                    case "Password2":
                        if (string.IsNullOrEmpty(Password1?.Trim()) || string.IsNullOrEmpty(Password2?.Trim()))
                            errorMessage = Resources.SID_Password_should_be_set;
                        else if (Password1 != Password2)
                            errorMessage = Resources.SID_Passwords_don_t_match;
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;

            }
        }

        public string Error { get; set; }
    }
}
