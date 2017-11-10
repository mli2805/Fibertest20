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
        public Guid ClientId { private get; set; }
        public int UserId { get; set; }

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
            IWcfServiceForClient c2DWcfManager)
        {
            _windowManager = windowManager;
            _iniFile = iniFile;
            _logFile = logFile;
            _c2DWcfManager = c2DWcfManager;
        }

        private void ParseServerAnswer(ClientRegisteredDto dto)
        {
            if (dto.ReturnCode == ReturnCode.ClientRegisteredSuccessfully)
            {
                UserId = dto.UserId;
                _logFile.AppendLine(@"Registered successfully");
                TryClose(true);
            }
            else
            {
                _logFile.AppendLine(dto.ReturnCode.ToString());
                Status = $@"Error = {dto.ReturnCode.GetLocalizedString()}";
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
                UserName = @"developer";
            if (string.IsNullOrEmpty(Password))
                Password = @"developer";
#endif
            _logFile.AppendLine(@"Client registration attempt");
            Status = Resources.SID_Client_registraion_is_performing;
            using (new WaitCursor())
            {
                var result = await RegisterClientAsync();
                ParseServerAnswer(result);
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
                    UserName = UserName,
                    Password = Password,
                });

            if (result.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
                MessageBox.Show(result.ReturnCode.GetLocalizedString(), Resources.SID_Error);
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
