using System;
using System.Windows;
using Caliburn.Micro;
using Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

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
        public string Status { get; set; } = Resources.SID_Input_user_name_and_password;


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

        public void Login()
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
                Status = Resources.SID_User_signed_in;
//                RegisterClient();
                var result = MakeExperiment();


                TestInitializationLongTask();


                ParseServerAnswer(result);
            }
        }

        private void TestInitializationLongTask()
        {
            var dcServiceAddresses = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            var c2DWcfManager = new C2DWcfManager(dcServiceAddresses, _iniFile, _logFile, _clientId);
            var dto = new InitializeRtuDto() {ServerAddresses = dcServiceAddresses};
//            c2DWcfManager.I
        }

        private ClientRegisteredDto MakeExperiment()
        {
            var dcServiceAddresses = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            var c2DWcfManager = new C2DWcfManager(dcServiceAddresses, _iniFile, _logFile, _clientId);
            var clientAddresses = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            var dto = new RegisterClientDto()
            {
                Addresses = new DoubleAddress() {Main = clientAddresses, HasReserveAddress = false},
                UserName = UserName
            };
            return c2DWcfManager.MakeExperiment(dto);
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

        private void RegisterClient()
        {
            var dcServiceAddresses = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            var c2DWcfManager = new C2DWcfManager(dcServiceAddresses, _iniFile, _logFile, _clientId);

            var clientAddresses = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            if (!c2DWcfManager.RegisterClient(
                new RegisterClientDto()
                {
                    Addresses = new DoubleAddress() { Main = clientAddresses, HasReserveAddress = false },
                    UserName = UserName
                }))
                MessageBox.Show(Resources.SID_Can_t_establish_connection_with_server_, Resources.SID_Error);
            Status = Resources.SID_Request_is_sent;
        }

        public void SetServerAddress()
        {
            var vm = new ServerConnectViewModel(_clientId, _iniFile, _logFile);
            _windowManager.ShowDialog(vm);
        }
    }
}
