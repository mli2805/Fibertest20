using System;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class LoginViewModel : Screen
    {
        private readonly Guid _clientId;
        private readonly IWindowManager _windowManager;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        public string UserName { get; set; }
        public string Password { get; set; }

        private string _status = Resources.SID_Input_user_name_and_password;
        public string Status
        {
            get { return _status; }
            set
            {
                if (value == _status) return;
                _status = value;
                NotifyOfPropertyChange();
            }
        }


        public LoginViewModel(Guid clientId, IWindowManager windowManager, IniFile iniFile, IMyLog logFile)
        {
            _clientId = clientId;
            _windowManager = windowManager;
            _iniFile = iniFile;
            _logFile = logFile;
        }

        private void ParseServerAnswer(ClientRegisteredDto dto)
        {
            if (dto.IsRegistered)
            {
                _logFile.AppendLine(@"Registered successfully");
                TryClose(true);
            }
            else
            {
                _logFile.AppendLine(@"Something goes wrong with registration");
                Status = $@"Error = {dto.ErrorCode}";
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Authentication;
        }

        public async void Login()
        {
#if DEBUG
            if (string.IsNullOrEmpty(UserName))
                UserName = @"root";
            if (string.IsNullOrEmpty(Password))
                Password = @"root";
#endif
            if (CheckPassword())
            {
                _logFile.AppendLine($@"User signed in as {UserName}");
                Status = Resources.SID_Client_registraion_is_performing;
                var result = await RegisterClientAsync();
                ParseServerAnswer(result);
            }
        }

        private bool CheckPassword()
        {
            if (UserName == @"root" && Password == @"root")
                return true;
            return CheckPasswordInDb();
        }

        private bool CheckPasswordInDb()
        {
            return true;
        }

        private async Task<ClientRegisteredDto> RegisterClientAsync()
        {
            var dcServiceAddresses = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            var c2DWcfManager = new C2DWcfManager(dcServiceAddresses, _iniFile, _logFile, _clientId);

            var clientAddresses = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            var result = await c2DWcfManager.RegisterClientAsync(
                new RegisterClientDto()
                {
                    Addresses = new DoubleAddress() {Main = clientAddresses, HasReserveAddress = false},
                    UserName = UserName
                });

            if (!result.IsRegistered)
                MessageBox.Show(Resources.SID_Can_t_establish_connection_with_server_, Resources.SID_Error);
            return result;
        }

        public void SetServerAddress()
        {
            var vm = new ServerConnectViewModel(_clientId, _iniFile, _logFile);
            _windowManager.ShowDialog(vm);
        }
    }
}
