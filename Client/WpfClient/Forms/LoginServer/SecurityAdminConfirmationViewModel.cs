using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class SecurityAdminConfirmationViewModel : Screen
    {
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public string Text3 { get; set; }
        public string Text4 { get; set; } = @"Please, input Security Administrator password";
        public string Password { get; set; }

        public bool IsOkPressed { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Password;
            Password = "";
        }

        public void Initialize(ClientRegisteredDto resultDto)
        {
            if (resultDto.ReturnCode == ReturnCode.WrongMachineKey)
            {
                Text1 = @"Wrong workstation key!";
                Text2 = @"Your credentials must be verified by Security Administrator.";
                Text3 = @"Ваши полномочия должны быть подтверждены администратором безопасности.";
            }
            else if (resultDto.ReturnCode == ReturnCode.EmptyMachineKey)
            {
                Text1 = @"Для данного пользователя не задана привязка к рабочему месту!";
                Text2 = @"Your credentials must be verified by Security Administrator.";
                Text3 = @"Ваши полномочия должны быть подтверждены администратором безопасности.";
            }
            else if (resultDto.ReturnCode == ReturnCode.WrongSecurityAdminPassword)
            {
                Text2 = @"Неверный пароль администратора безопасности!";
            }
        }

        public void Initialize()
        {
            Text1 = @"Данная лицензия требует привязки пользователей и рабочих мест";
            Text2 = @"Для применения данной лицензии требуется ввод пароля администратора безопасности";
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
