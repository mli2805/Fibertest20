using System;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class LoginViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IMachineKeyProvider _machineKeyProvider;
        private readonly NoLicenseAppliedViewModel _noLicenseAppliedViewModel;
        private readonly SecurityAdminConfirmationViewModel _securityAdminConfirmationViewModel;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IWcfServiceDesktopC2D _desktopC2DWcfManager;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly CurrentUser _currentUser;
        private readonly CurrentGis _currentGis;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;

        private string _userName;
        public string UserName
        {
            get => _userName;
            set { _userName = value; Status = Resources.SID_Input_user_name_and_password; }
        }

        public PasswordViewModel PasswordViewModel { get; set; } = new PasswordViewModel();

        public string ConnectionId { get; set; }

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

        public bool IsRegistrationSuccessful { get; set; }

        public LoginViewModel(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile,
            IWindowManager windowManager, SecurityAdminConfirmationViewModel securityAdminConfirmationViewModel,
            IMachineKeyProvider machineKeyProvider, NoLicenseAppliedViewModel noLicenseAppliedViewModel,
            IWcfServiceDesktopC2D desktopC2DWcfManager, IWcfServiceCommonC2D commonC2DWcfManager,
            CurrentUser currentUser, CurrentGis currentGis, CurrentDatacenterParameters currentDatacenterParameters)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _machineKeyProvider = machineKeyProvider;
            _noLicenseAppliedViewModel = noLicenseAppliedViewModel;
            _securityAdminConfirmationViewModel = securityAdminConfirmationViewModel;
            _iniFile = iniFile;
            _logFile = logFile;
            _desktopC2DWcfManager = desktopC2DWcfManager;
            _commonC2DWcfManager = commonC2DWcfManager;
            _currentUser = currentUser;
            _currentGis = currentGis;
            _currentDatacenterParameters = currentDatacenterParameters;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Authentication;
        }

        public async void Login()
        {
#if DEBUG
            if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(PasswordViewModel.Password))
            {
                // UserName = @"superclient";  PasswordViewModel.Password = @"superclient";
                UserName = @"developer"; PasswordViewModel.Password = @"developer";
                // UserName = @"operator"; PasswordViewModel.Password = @"operator";
                // UserName = @"supervisor"; PasswordViewModel.Password = @"supervisor";
                //  UserName = @"root"; PasswordViewModel.Password = @"root";
            }
#endif
            if (string.IsNullOrEmpty(ConnectionId))
                ConnectionId = Guid.NewGuid().ToString();

            var unused = await RegisterClientAsync(UserName, PasswordViewModel.Password, ConnectionId);
        }

        private DoubleAddress _commonServiceAddresses;
        private DoubleAddress _desktopServiceAddresses;
        private NetAddress _clientAddress;
        private RegisterClientDto _sendDto;

        private void PrepareAddresses(string username, bool isUnderSuperClient = false, int ordinal = 0)
        {
            _desktopServiceAddresses = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToDesktopClient);
            _currentDatacenterParameters.ServerIp = _desktopServiceAddresses.Main.Ip4Address;
            _currentDatacenterParameters.ServerTitle = _iniFile.Read(IniSection.Server, IniKey.ServerTitle, "");

            var clientTcpPort = (int)TcpPorts.ClientListenTo;
            if (isUnderSuperClient) clientTcpPort += ordinal;
            _clientAddress = _iniFile.Read(IniSection.ClientLocalAddress, clientTcpPort);
            if (_clientAddress.IsAddressSetAsIp && _clientAddress.Ip4Address == @"0.0.0.0" &&
                _desktopServiceAddresses.Main.Ip4Address != @"0.0.0.0")
            {
                _clientAddress.Ip4Address = LocalAddressResearcher.GetLocalAddressToConnectServer(_desktopServiceAddresses.Main.Ip4Address);
                _iniFile.Write(_clientAddress, IniSection.ClientLocalAddress);
            }

            _commonServiceAddresses = (DoubleAddress)_desktopServiceAddresses.Clone();
            _commonServiceAddresses.Main.Port = (int)TcpPorts.ServerListenToCommonClient;
            if (_commonServiceAddresses.HasReserveAddress)
                _commonServiceAddresses.Reserve.Port = (int)TcpPorts.ServerListenToCommonClient;

            _desktopC2DWcfManager.SetServerAddresses(_desktopServiceAddresses, username, _clientAddress.Ip4Address);
            _commonC2DWcfManager.SetServerAddresses(_commonServiceAddresses, username, _clientAddress.Ip4Address);
        }

        // public to start under super-client
        public async Task<bool> RegisterClientAsync(string username, string password,
            string connectionId, bool isUnderSuperClient = false, int ordinal = 0)
        {
            PrepareAddresses(username, isUnderSuperClient, ordinal);

            Status = string.Format(Resources.SID_Performing_registration_on__0_, _desktopServiceAddresses.Main.Ip4Address);
            _logFile.AppendLine($@"Performing registration on {_desktopServiceAddresses.Main.Ip4Address}");

            _sendDto = new RegisterClientDto()
            {
                UserName = username,
                Password = password.GetHashString(),
                ConnectionId = connectionId,
                MachineKey = _machineKeyProvider.Get(),
                IsUnderSuperClient = isUnderSuperClient,
                Addresses = new DoubleAddress() { Main = _clientAddress, HasReserveAddress = false }
            };

            return await ProcessRegistrationResult();
        }

        private async Task<bool> ProcessRegistrationResult()
        {
            var resultDto = await SendAsync(_sendDto);
            if (resultDto.ReturnCode == ReturnCode.NoLicenseHasBeenAppliedYet)
            {
                _windowManager.ShowDialog(_noLicenseAppliedViewModel);
                if (!_noLicenseAppliedViewModel.IsCommandSent)
                    return false;
                if (!_noLicenseAppliedViewModel.IsLicenseAppliedSuccessfully)
                    return false; // message was shown already

                _sendDto.SecurityAdminPassword = _noLicenseAppliedViewModel.SecurityAdminPassword.GetHashString();
                resultDto = await SendAsync(_sendDto);
            }
            else if (resultDto.ReturnCode == ReturnCode.WrongMachineKey
                     || resultDto.ReturnCode == ReturnCode.EmptyMachineKey
                     || resultDto.ReturnCode == ReturnCode.WrongSecurityAdminPassword)
            {
                if (!AskSecurityAdminPassword(resultDto))
                   return false;

                _sendDto.SecurityAdminPassword = _securityAdminConfirmationViewModel.PasswordViewModel.Password.GetHashString();
                resultDto = await SendAsync(_sendDto);
            }

            if (resultDto.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, resultDto.ReturnCode.GetLocalizedString());
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }

            ParseServerAnswer(resultDto);
            return true;
        }

        private async Task<ClientRegisteredDto> SendAsync(RegisterClientDto dto)
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                return await _commonC2DWcfManager.RegisterClientAsync(dto);
            }
        }

        private bool AskSecurityAdminPassword(ClientRegisteredDto resultDto)
        {
            _securityAdminConfirmationViewModel.Initialize(resultDto);
            _windowManager.ShowDialog(_securityAdminConfirmationViewModel);
            return _securityAdminConfirmationViewModel.IsOkPressed;
        }

        public void ChooseServer()
        {
            var vm = _globalScope.Resolve<ServersConnectViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        private void ParseServerAnswer(ClientRegisteredDto result)
        {
            if (result.ReturnCode == ReturnCode.ClientRegisteredSuccessfully)
            {
                _currentUser.FillIn(result);
                _currentUser.UserName = UserName;
                _currentDatacenterParameters.FillIn(result);
                _currentGis.IsWithoutMapMode = result.IsWithoutMapMode;

                _logFile.AppendLine(@"Registered successfully");
                _logFile.AppendLine($@"StreamIdOriginal = {result.StreamIdOriginal}  Last event number in snapshot {result.SnapshotLastEvent}");
                IsRegistrationSuccessful = true;
                TryClose();
            }
            else
            {
                _logFile.AppendLine(result.ReturnCode.ToString());
                Status = result.ReturnCode.GetLocalizedString(result.ErrorMessage);
            }
        }

        public void Cancel()
        {
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            if (!IsRegistrationSuccessful)
            {
                var question = Resources.SID_Close_application_;
                var vm = new MyMessageBoxViewModel(MessageType.Confirmation, question);
                _windowManager.ShowDialogWithAssignedOwner(vm);

                if (!vm.IsAnswerPositive) return;
            }
            base.CanClose(callback);
        }
    }
}
