using System.ComponentModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class ChangePasswordViewModel : Screen, IDataErrorInfo
    {
        private readonly IWcfServiceForClient _c2DWcfManager;

        private string _inputPassword;
        public string InputPassword
        {
            get => _inputPassword;
            set
            {
                if (value == _inputPassword) return;
                _inputPassword = value;
                Explanation = "";
                NotifyOfPropertyChange();
            }
        }

        private User _user;

        private bool _isChangePasswordEnabled;
        public bool IsChangePasswordEnabled
        {
            get => _isChangePasswordEnabled;
            set
            {
                if (value == _isChangePasswordEnabled) return;
                _isChangePasswordEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        private string _password1;
        public string Password1
        {
            get => _password1;
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
            get => _password2;
            set
            {
                if (value == _password2) return;
                _password2 = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Password1));
            }
        }

        private bool _isButtonSaveEnabled;
        public bool IsButtonSaveEnabled
        {
            get => _isButtonSaveEnabled;
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _newPasswordBlockVisibility = Visibility.Collapsed;
        public Visibility NewPasswordBlockVisibility
        {
            get => _newPasswordBlockVisibility;
            set
            {
                if (value == _newPasswordBlockVisibility) return;
                _newPasswordBlockVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isNewPasswordFocused;
        public bool IsNewPasswordFocused
        {
            get => _isNewPasswordFocused;
            set
            {
                if (value == _isNewPasswordFocused) return;
                _isNewPasswordFocused = value;
                NotifyOfPropertyChange();
            }
        }

        private string _explanation;

        public string Explanation
        {
            get => _explanation;
            set
            {
                if (value == _explanation) return;
                _explanation = value;
                NotifyOfPropertyChange();
            }
        }

        public ChangePasswordViewModel(IWcfServiceForClient c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
        }

        public void Initialize(User user)
        {
            _user = user;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Change_password;
        }

        public void CompareWithCurrent()
        {
            IsChangePasswordEnabled = UserExt.FlipFlop(_user.EncodedPassword) == InputPassword;
            Explanation = IsChangePasswordEnabled ? "" : Resources.SID_Wrong_password;
            NewPasswordBlockVisibility = IsChangePasswordEnabled ? Visibility.Visible : Visibility.Collapsed;
            IsNewPasswordFocused = IsChangePasswordEnabled;
            IsButtonSaveEnabled = false;
        }

        public async void Save()
        {
            var cmd = new UpdateUser()
            {
                UserId = _user.UserId,
                Title = _user.Title,
                Role = _user.Role,
                Email = _user.Email,
                IsEmailActivated = _user.IsEmailActivated,
                EncodedPassword = UserExt.FlipFlop(Password1),
                ZoneId = _user.ZoneId,
            };
            await _c2DWcfManager.SendCommandAsObj(cmd);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
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
                        break;
                }
                IsButtonSaveEnabled = errorMessage == string.Empty;
                return errorMessage;
            }
        }

        public string Error { get; set; }
    }
}
