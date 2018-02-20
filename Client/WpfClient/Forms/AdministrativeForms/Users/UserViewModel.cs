using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class UserViewModel : Screen, IDataErrorInfo
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private bool _isInCreationMode;

        private readonly ReadModel _readModel;
        public UserVm UserInWork { get; set; }

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


        public List<Role> Roles { get; set; }
        public List<Zone> Zones { get; set; }

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

        public UserViewModel(IWcfServiceForClient c2DWcfManager, ReadModel readModel)
        {
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
        }

        public void Initialize()
        {
            _isInCreationMode = true;
            UserInWork = new UserVm();

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Skip(2).ToList();
            IsntItRoot = true;

            Zones = _readModel.Zones;
            SelectedZone = Zones.First();
        }
        public void Initialize(UserVm user)
        {
            _isInCreationMode = false;

            UserInWork = user;

            if (UserInWork.Role == Role.Root)
            {
                Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Skip(1).ToList();
                IsntItRoot = false;
            }
            else
            {
                Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Skip(2).ToList();
                IsntItRoot = true;
            }
            if (UserInWork.Role == 0)
                UserInWork.Role = Roles.First();
            Password1 = Password2 = UserInWork.Password;

            Zones = _readModel.Zones;
            SelectedZone = (UserInWork.IsDefaultZoneUser) ? Zones.First() : Zones.First(z=>z.ZoneId == user.ZoneId);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInCreationMode ? Resources.SID_New_user_creation : Resources.SID_Update_user_info;
        }

        public async void Save()
        {
            object cmd;
            if (_isInCreationMode)
                cmd = new AddUser()
                {
                    UserId = Guid.NewGuid(),
                    Title = UserInWork.Title,
                    Role = UserInWork.Role,
                    Email = UserInWork.Email,
                    IsEmailActivated = UserInWork.IsEmailActivated,
                    EncodedPassword = UserExt.FlipFlop(UserInWork.Password),
                    IsDefaultZoneUser = UserInWork.IsDefaultZoneUser,
                    ZoneId = UserInWork.ZoneId,
                };
            else
                cmd = new UpdateUser()
                {
                    UserId = UserInWork.UserId,
                    Title = UserInWork.Title,
                    Role = UserInWork.Role,
                    Email = UserInWork.Email,
                    IsEmailActivated = UserInWork.IsEmailActivated,
                    EncodedPassword = UserExt.FlipFlop(UserInWork.Password),
                    IsDefaultZoneUser = UserInWork.IsDefaultZoneUser,
                    ZoneId = UserInWork.ZoneId,
                };

            await _c2DWcfManager.SendCommandAsObj(cmd);
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
                    case "Title":
                        if (string.IsNullOrEmpty(UserInWork.Title?.Trim()))
                            errorMessage = Resources.SID_Title_should_be_set_;
                        break;
                    case "Password1":
                    case "Password2":
                        if (string.IsNullOrEmpty(Password1?.Trim()) || string.IsNullOrEmpty(Password2?.Trim()))
                            errorMessage = Resources.SID_Password_should_be_set;
                        else if (Password1 != Password2)
                            errorMessage = Resources.SID_Passwords_don_t_match;
                        break;
                }
                IsButtonSaveEnabled = errorMessage == string.Empty;
                return errorMessage;

            }
        }

        public string Error { get; set; }
    }
}
