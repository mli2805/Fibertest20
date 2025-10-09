using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class SnmpSettingsViewModel : Screen
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public bool Enabled { get; set; }

        public List<string> SnmpVersions { get; set; } = new List<string>() { @"v1", @"v3" };

        private string _selectedSnmpVersion;
        public string SelectedSnmpVersion
        {
            get => _selectedSnmpVersion;
            set
            {
                if (value == _selectedSnmpVersion) return;
                _selectedSnmpVersion = value;
                NotifyOfPropertyChange();
            }
        }


        private bool _useIitOid;
        public bool UseIitOid
        {
            get => _useIitOid;
            set
            {
                if (value == _useIitOid) return;
                _useIitOid = value;
                EnableCustomId = !_useIitOid;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(EnableCustomId));
            }
        }

        public bool EnableCustomId { get; set; }

        public string CustomOid { get; set; }

        public string Community { get; set; }
        public string AuthoritativeEngineId { get; set; }
        public string UserName { get; set; }
        public bool IsAuthPswSet { get; set; }
        public string AuthenticationPassword { get; set; }
        public List<string> AuthenticationProtocols { get; set; } = new List<string>() { @"Md5", @"Sha", @"Sha256", @"Sha384", @"Sha512" };
        public string SelectedAuthenticationProtocol { get; set; }
        public bool IsPrivPswSet { get; set; }
        public string PrivacyPassword { get; set; }
        public List<string> PrivacyProtocols { get; set; } = new List<string>()
        {
            @"None", @"Des", @"TripleDes", @"Aes128", @"Aes192", @"Aes256"
        };
        public string SelectedPrivacyProtocol { get; set; }
        public string TrapReceiverAddress { get; set; }
        public int TrapReceiverPort { get; set; }

        public List<string> SnmpEncodings { get; set; } = new List<string>() { @"utf8", @"windows1251" };
        public string SelectedSnmpEncoding { get; set; }

        public bool IsEditEnabled { get; set; }

        public SnmpSettingsViewModel(Model readModel, CurrentUser currentUser,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager)
        {
            IsEditEnabled = currentUser.Role <= Role.Root;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            var snmp = readModel.SnmpNewSettings;

            Enabled = snmp.Enabled;
            SelectedSnmpVersion = snmp.SnmpVersion;
            UseIitOid = snmp.UseIitOid;
            CustomOid = snmp.CustomOid;
            Community = snmp.Community;
            AuthoritativeEngineId = snmp.AuthoritativeEngineId;
            UserName = snmp.UserName;
            IsAuthPswSet = snmp.IsAuthPswSet;
            AuthenticationPassword = snmp.AuthenticationPassword;
            SelectedAuthenticationProtocol = snmp.AuthenticationProtocol;
            IsPrivPswSet = snmp.IsPrivPswSet;
            PrivacyPassword = snmp.PrivacyPassword;
            SelectedPrivacyProtocol = snmp.PrivacyProtocol;
            TrapReceiverAddress = snmp.TrapReceiverAddress;
            TrapReceiverPort = snmp.TrapReceiverPort;
            SelectedSnmpEncoding = snmp.SnmpEncoding;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_SNMP_settings;
        }

        public async void SaveAndTest()
        {
            bool res;
            using (new WaitCursor())
            {
                var saveResult = await SaveSettings();
                res = saveResult == null;

                // только если сохранение успешно, начинаем тестить
                if (res)
                {
                    var dto = new SnmpNewSettingsDto()
                    {
                        Enabled = Enabled,
                        SnmpVersion = SelectedSnmpVersion,
                        UseIitOid = UseIitOid,
                        CustomOid = CustomOid,
                        Community = Community,
                        AuthoritativeEngineId = AuthoritativeEngineId,
                        UserName = UserName,
                        IsAuthPswSet = IsAuthPswSet,
                        AuthenticationPassword = AuthenticationPassword,
                        AuthenticationProtocol = SelectedAuthenticationProtocol,
                        IsPrivPswSet = IsPrivPswSet,
                        PrivacyPassword = PrivacyPassword,
                        PrivacyProtocol = SelectedPrivacyProtocol,
                        TrapReceiverAddress = TrapReceiverAddress,
                        TrapReceiverPort = TrapReceiverPort,
                        SnmpEncoding = SelectedSnmpEncoding
                    };

                    res = await _c2DWcfManager.TestSnmpNewSettings(dto);
                }
            }

            if (res)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Information, new List<string>()
                {
                    Resources.SID_SNMP_trap_sent_,
                    Resources.SID_Make_sure_if_trap_has_been_received
                });
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_send_SNMP_trap_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        private async Task<string> SaveSettings()
        {
            var settings = new SnmpNewSettings()
            {
                Enabled = Enabled,
                SnmpVersion = SelectedSnmpVersion,
                UseIitOid = UseIitOid,
                CustomOid = CustomOid,
                Community = Community,
                AuthoritativeEngineId = AuthoritativeEngineId,
                UserName = UserName,
                IsAuthPswSet = IsAuthPswSet,
                AuthenticationPassword = AuthenticationPassword,
                AuthenticationProtocol = SelectedAuthenticationProtocol,
                IsPrivPswSet = IsPrivPswSet,
                PrivacyPassword = PrivacyPassword,
                PrivacyProtocol = SelectedPrivacyProtocol,
                TrapReceiverAddress = TrapReceiverAddress,
                TrapReceiverPort = TrapReceiverPort,
                SnmpEncoding = SelectedSnmpEncoding
            };
            var command = new UpdateSnmpNewSettings() { SnmpNewSettings = settings };
            return await _c2DWcfManager.SendCommandAsObj(command);
        }

        public void Close()
        {
            TryClose();
        }
    }
}
