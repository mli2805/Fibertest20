using System;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class LoginViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly AdministrativeDb _administrativeDb;
        public Guid ClientId { private get; set; }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; Status = Resources.SID_Input_user_name_and_password; }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; Status = Resources.SID_Input_user_name_and_password; }
        }

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


        public LoginViewModel(IWindowManager windowManager, IniFile iniFile, IMyLog logFile,
            IWcfServiceForClient c2DWcfManager, AdministrativeDb administrativeDb)
        {
            _windowManager = windowManager;
            _iniFile = iniFile;
            _logFile = logFile;
            _c2DWcfManager = c2DWcfManager;
            _administrativeDb = administrativeDb;
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
            if (_administrativeDb.CheckPassword(UserName, Password))
            {
                _logFile.AppendLine($@"User signed in as {UserName}");
                Status = Resources.SID_Client_registraion_is_performing;
                var result = await RegisterClientAsync();
                ParseServerAnswer(result);
            }
            else
            {
                Status = Resources.SID_Wrong_user_name_or_password_;
            }
        }

        private async Task<ClientRegisteredDto> RegisterClientAsync()
        {
            var dcServiceAddresses = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            ((C2DWcfManager)_c2DWcfManager).SetServerAddresses(dcServiceAddresses);
            ((C2DWcfManager)_c2DWcfManager).ClientId = ClientId;

            var clientAddresses = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            var result = await _c2DWcfManager.RegisterClientAsync(
                new RegisterClientDto()
                {
                    Addresses = new DoubleAddress() { Main = clientAddresses, HasReserveAddress = false },
                    UserName = UserName
                });

            if (!result.IsRegistered)
                MessageBox.Show(Resources.SID_Can_t_establish_connection_with_server_, Resources.SID_Error);
            return result;
        }

        public void SetServerAddress()
        {
            //            var vm = new ServerConnectViewModel(_c2DWcfManager, _iniFile);
            var vm = IoC.Get<ServerConnectViewModel>();
            _windowManager.ShowDialog(vm);
        }
    }
}
