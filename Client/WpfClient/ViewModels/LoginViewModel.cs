using System;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class LoginViewModel : Screen
    {
        private readonly Guid _clientId;
        private readonly IWindowManager _windowManager;
        private readonly IniFile _iniFile;
        private readonly LogFile _logFile;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Status { get; set; } = Resources.SID_Input_user_name_and_password;


        public LoginViewModel(Guid clientId, IWindowManager windowManager, IniFile iniFile, LogFile logFile)
        {
            _clientId = clientId;
            _windowManager = windowManager;
            _iniFile = iniFile;
            _logFile = logFile;
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
            var vm = new ServerConnectViewModel(_clientId, _iniFile, _logFile);
            _windowManager.ShowDialog(vm);
        }
    }
}
