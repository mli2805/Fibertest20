using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class LoginViewModel : Screen
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Status { get; set; } = Resources.SID_Input_user_name_and_password;
        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Authentication;
        }

        public void Login()
        {
            
        }

        public void ConnectServer()
        {
            
        }
    }
}
