﻿using System;
using System.Threading.Tasks;
using System.Windows;
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

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; Status = Resources.SID_Input_user_name_and_password; }
        }

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
            IWindowManager windowManager, NoLicenseAppliedViewModel noLicenseAppliedViewModel,
            SecurityAdminConfirmationViewModel securityAdminConfirmationViewModel,
            IWcfServiceDesktopC2D desktopC2DWcfManager, IWcfServiceCommonC2D commonC2DWcfManager,
            CurrentUser currentUser, CurrentGis currentGis,
            CurrentDatacenterParameters currentDatacenterParameters)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
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
            if (string.IsNullOrEmpty(UserName))
                //                UserName = @"superclient";
                UserName = @"developer";
            //                                            UserName = @"operator";
            //                UserName = @"supervisor";
            //            UserName = @"root";
            if (string.IsNullOrEmpty(Password))
                //                Password = @"superclient";
                //                Password = @"1";
                Password = @"developer";
            //                                            Password = @"operator";
            //                Password = @"supervisor";
            //            Password = @"root";
#endif
            if (string.IsNullOrEmpty(ConnectionId))
                ConnectionId = Guid.NewGuid().ToString();

            var unused = await RegisterClientAsync(UserName, Password, ConnectionId);
        }

        // public to start under super-client
        public async Task<bool> RegisterClientAsync(string username, string password,
            string connectionId, bool isUnderSuperClient = false, int ordinal = 0)
        {
            var dcServiceAddresses = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToDesktopClient);
            _currentDatacenterParameters.ServerIp = dcServiceAddresses.Main.Ip4Address;
            _currentDatacenterParameters.ServerTitle = _iniFile.Read(IniSection.Server, IniKey.ServerTitle, "");
            var clientPort = (int)TcpPorts.ClientListenTo;
            if (isUnderSuperClient) clientPort += ordinal;
            Status = string.Format(Resources.SID_Performing_registration_on__0_, dcServiceAddresses.Main.Ip4Address);
            _logFile.AppendLine($@"Performing registration on {dcServiceAddresses.Main.Ip4Address}");

            var sendDto = new RegisterClientDto()
            {
                UserName = username,
                Password = password,
                ConnectionId = connectionId,
                IsUnderSuperClient = isUnderSuperClient,
            };

            var resultDto = await PureRegisterClientAsync(dcServiceAddresses, clientPort, sendDto);

            if (resultDto.ReturnCode == ReturnCode.NoLicenseHasBeenAppliedYet)
            {
                _windowManager.ShowDialog(_noLicenseAppliedViewModel);
                if (!_noLicenseAppliedViewModel.IsCommandSent)
                {
                    var vm = new MyMessageBoxViewModel(MessageType.Error,
                        @"Enter program without license applied prohibited");
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return false;
                }
                if (!_noLicenseAppliedViewModel.IsLicenseAppliedSuccessfully)
                    return false; // message was shown already

                resultDto = await PureRegisterClientAsync(dcServiceAddresses, clientPort, sendDto);

            }
            else if (resultDto.ReturnCode == ReturnCode.WrongMachineKey
                     || resultDto.ReturnCode == ReturnCode.WrongSecurityAdminPassword)
            {
                if (!AskSecurityAdminConfirmation(resultDto))
                {
                    resultDto.ReturnCode = ReturnCode.WrongMachineKey;
                }
            }
            else if (resultDto.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
                MessageBox.Show(resultDto.ReturnCode.GetLocalizedString(), Resources.SID_Error_);

            ParseServerAnswer(resultDto);
            return true;
        }

        private bool AskSecurityAdminConfirmation(ClientRegisteredDto resultDto)
        {
            _securityAdminConfirmationViewModel.Initialize(resultDto);
            _windowManager.ShowDialog(_securityAdminConfirmationViewModel);
            return true;
        }

        private async Task<ClientRegisteredDto> PureRegisterClientAsync(
            DoubleAddress dcServiceAddresses, int clientTcpPort, RegisterClientDto dto)
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                var clientAddress = _iniFile.Read(IniSection.ClientLocalAddress, clientTcpPort);
                if (clientAddress.IsAddressSetAsIp && clientAddress.Ip4Address == @"0.0.0.0" &&
                    dcServiceAddresses.Main.Ip4Address != @"0.0.0.0")
                {
                    clientAddress.Ip4Address = LocalAddressResearcher.GetLocalAddressToConnectServer(dcServiceAddresses.Main.Ip4Address);
                    _iniFile.Write(clientAddress, IniSection.ClientLocalAddress);
                }

                _desktopC2DWcfManager.SetServerAddresses(dcServiceAddresses, dto.UserName, clientAddress.Ip4Address);
                var da = (DoubleAddress)dcServiceAddresses.Clone();
                da.Main.Port = (int)TcpPorts.ServerListenToCommonClient;
                if (da.HasReserveAddress) da.Reserve.Port = (int)TcpPorts.ServerListenToCommonClient;
                _commonC2DWcfManager.SetServerAddresses(da, dto.UserName, clientAddress.Ip4Address);
                dto.Addresses = new DoubleAddress() { Main = clientAddress, HasReserveAddress = false };
                var result = await _commonC2DWcfManager.RegisterClientAsync(dto);

                return result;
            }


        }

        public void SetServerAddress()
        {
            var vm = _globalScope.Resolve<ServersConnectViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
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
                _currentUser.ConnectionId = result.ConnectionId;
                _currentDatacenterParameters.DatacenterVersion = result.DatacenterVersion;
                _currentDatacenterParameters.StreamIdOriginal = result.StreamIdOriginal;
                _currentDatacenterParameters.SnapshotLastEvent = result.SnapshotLastEvent;
                _currentDatacenterParameters.SnapshotLastDate = result.SnapshotLastDate;
                _currentDatacenterParameters.Smtp = new SmtpSettingsDto()
                {
                    SmptHost = result.Smtp.SmptHost,
                    SmptPort = result.Smtp.SmptPort,
                    MailFrom = result.Smtp.MailFrom,
                    MailFromPassword = result.Smtp.MailFromPassword,
                    SmtpTimeoutMs = result.Smtp.SmtpTimeoutMs,
                };
                _currentDatacenterParameters.GsmModemComPort = result.GsmModemComPort;
                _currentDatacenterParameters.Snmp = new SnmpSettingsDto()
                {
                    IsSnmpOn = result.Snmp.IsSnmpOn,
                    SnmpTrapVersion = result.Snmp.SnmpTrapVersion,
                    SnmpReceiverIp = result.Snmp.SnmpReceiverIp,
                    SnmpReceiverPort = result.Snmp.SnmpReceiverPort,
                    SnmpAgentIp = result.Snmp.SnmpAgentIp,
                    SnmpCommunity = result.Snmp.SnmpCommunity,
                    EnterpriseOid = result.Snmp.EnterpriseOid,
                    SnmpEncoding = result.Snmp.SnmpEncoding,
                };
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
