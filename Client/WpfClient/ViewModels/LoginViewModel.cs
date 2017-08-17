using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class LoginViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly IniFile _iniFile;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Status { get; set; } = Resources.SID_Input_user_name_and_password;


        public LoginViewModel(IWindowManager windowManager, IniFile iniFile)
        {
            _windowManager = windowManager;
            _iniFile = iniFile;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Authentication;
        }

        public void Login()
        {
            TryClose(true);
        }

        public void ConnectServer()
        {
            var vm = new ServerConnectViewModel(_iniFile);
            _windowManager.ShowDialog(vm);
        }
    }
}
