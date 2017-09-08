using System;
using System.Windows;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
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

            ClientWcfService.MessageReceived += ClientWcfService_MessageReceived;
        }

        private void ClientWcfService_MessageReceived(object e)
        {
            var dto = e as ClientRegisteredDto;
            if (dto != null)
                ParseServerAnswer(dto);
        }

        private void ParseServerAnswer(ClientRegisteredDto dto)
        {
            if (dto.IsRegistered)
                    TryClose(true);
            else
            {
                Status = $"Error = {dto.ErrorCode}";
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Authentication;
        }

        public void Login()
        {
            if (CheckPassword())
                RegisterClient();
        }

        private bool CheckPassword()
        {
            Status = "User accepted";
            return true;
        }

        private void RegisterClient()
        {
            var dcServiceAddresses = _iniFile.ReadDoubleAddress(11840);
            var clientAddresses = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);

            var c2DWcfManager = new C2DWcfManager(dcServiceAddresses, _iniFile, _logFile, _clientId);

            if (!c2DWcfManager.RegisterClient(new RegisterClientDto()
                { Addresses = new DoubleAddress() { Main = clientAddresses, HasReserveAddress = false }, UserName = UserName }))
                MessageBox.Show(@"Cannot register on server!");
            Status = "Request is sent";
        }

        public void SetServerAddress()
        {
            var vm = new ServerConnectViewModel(_clientId, _iniFile, _logFile);
            _windowManager.ShowDialog(vm);
        }
    }
}
