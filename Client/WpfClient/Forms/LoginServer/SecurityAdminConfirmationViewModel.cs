using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class SecurityAdminConfirmationViewModel : Screen
    {
        public string ReturnCodeText { get; set; }
        public string Text3 { get; set; }
        public string Text4 { get; set; }

        public PasswordViewModel PasswordViewModel { get; set; } = new PasswordViewModel();

        public bool IsOkPressed { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Password;
            PasswordViewModel.Password = "";
        }

        public void Initialize(ClientRegisteredDto resultDto)
        {
            ReturnCodeText = resultDto.ReturnCode.GetLocalizedString();
            if (resultDto.ReturnCode == ReturnCode.WrongMachineKey ||
                resultDto.ReturnCode == ReturnCode.EmptyMachineKey)
            {
                Text3 = Resources.SID_To_link_user_to_the_workstation_your_privileges;
                Text4 = Resources.SID_have_to_be_confirmed_by_security_administrator_password_;
            }
        }

        public void Initialize()
        {
            Text3 = Resources.SID_To_apply_the_license_security_administrator;
            Text4 = Resources.SID_password_has_to_be_input_;
        }

        public void OkButton()
        {
            IsOkPressed = true;
            TryClose();
        }

        public void CancelButton()
        {
            IsOkPressed = false;
            TryClose();
        }

    }
}
