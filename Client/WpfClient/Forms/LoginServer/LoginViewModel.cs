using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class LoginViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly CurrentUser _currentUser;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;

        private string _userName;
        public string UserName
        {
            get => _userName;
            set { _userName = value; Status = Resources.SID_Input_user_name_and_password; }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; Status = Resources.SID_Input_user_name_and_password; }
        }

        private string _status = Resources.SID_Input_user_name_and_password;
        public string Status
        {
            get => _status;
            set
            {
                if (value == _status) return;
                _status = value;
                NotifyOfPropertyChange();
            }
        }

        public LoginViewModel(ILifetimeScope globalScope, IWindowManager windowManager, IniFile iniFile, IMyLog logFile,
            IWcfServiceForClient c2DWcfManager, CurrentUser currentUser, CurrentDatacenterParameters currentDatacenterParameters)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _iniFile = iniFile;
            _logFile = logFile;
            _c2DWcfManager = c2DWcfManager;
            _currentUser = currentUser;
            _currentDatacenterParameters = currentDatacenterParameters;
        }

        private void ParseServerAnswer(ClientRegisteredDto result)
    {
            if (result.ReturnCode == ReturnCode.ClientRegisteredSuccessfully)
            {
                _currentUser.UserId = result.UserId;
                _currentUser.UserName = UserName;
                _currentUser.Role = result.Role;
                _currentUser.ZoneId = result.ZoneId;
                _currentUser.ZoneTitle = result.ZoneTitle;
                _currentDatacenterParameters.DatacenterVersion = result.DatacenterVersion;
                _currentDatacenterParameters.GraphDbVersionId = result.GraphDbVersionId;
                _logFile.AppendLine(@"Registered successfully");
                TryClose(true);
            }
            else
            {
                _logFile.AppendLine(result.ReturnCode.ToString());
                Status = result.ReturnCode.GetLocalizedString();
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
//                UserName = @"developer";
                UserName = @"operator";
//                UserName = @"root";
//                UserName = @"Op";
            if (string.IsNullOrEmpty(Password))
//                Password = @"developer";
                Password = @"operator";
//                Password = @"root";
//                Password = @"1";
#endif
            _logFile.AppendLine(@"Client registration attempt");
            Status = Resources.SID_Client_registraion_is_performing;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                var result = await RegisterClientAsync();
                ParseServerAnswer(result);
            }
        }

        private async Task<ClientRegisteredDto> RegisterClientAsync()
        {
            var dcServiceAddresses = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            var clientAddress = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            if (clientAddress.IsAddressSetAsIp && clientAddress.Ip4Address == @"0.0.0.0" &&
                dcServiceAddresses.Main.Ip4Address != @"0.0.0.0")
            {
                clientAddress.Ip4Address = LocalAddressResearcher.GetLocalAddressToConnectServer(dcServiceAddresses.Main.Ip4Address);
                _iniFile.Write(clientAddress, IniSection.ClientLocalAddress);
            }

            _c2DWcfManager.SetServerAddresses(dcServiceAddresses, UserName, clientAddress.Ip4Address);

            var result = await _c2DWcfManager.RegisterClientAsync(
                new RegisterClientDto()
                {
                    Addresses = new DoubleAddress() { Main = clientAddress, HasReserveAddress = false },
                    UserName = UserName,
                    Password = Password,
                });

            if (result.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
                MessageBox.Show(result.ReturnCode.GetLocalizedString(), Resources.SID_Error_);
            return result;
        }

        public void SetServerAddress()
        {
            var vm = _globalScope.Resolve<ServersConnectViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void Cancel() { TryClose(false);}
    }
}
