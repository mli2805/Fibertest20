using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class SnmpSettingsViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
     
        private bool _isSnmpOn;
        public bool IsSnmpOn
        {
            get => _isSnmpOn;
            set
            {
                if (value == _isSnmpOn) return;
                _isSnmpOn = value;
                NotifyOfPropertyChange();
            }
        }

        public string SnmpManagerIp { get; set; }
        public int SnmpManagerPort { get; set; }
        public string SnmpCommunity { get; set; }
        public string SnmpAgentIp { get; set; }

        public List<string> SnmpEncodings { get; set; } = new List<string>() { @"unicode (utf16)", @"utf8", @"windows1251" };
        public string SelectedSnmpEncoding { get; set; }

        public string EnterpriseOid { get; set; }

        public bool IsEditEnabled { get; set; }

        public SnmpSettingsViewModel(CurrentDatacenterParameters currentDatacenterParameters, CurrentUser currentUser,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            IsSnmpOn = currentDatacenterParameters.Snmp.IsSnmpOn;
            SnmpManagerIp = currentDatacenterParameters.Snmp.SnmpReceiverIp;
            SnmpManagerPort = currentDatacenterParameters.Snmp.SnmpReceiverPort;
            SnmpCommunity = currentDatacenterParameters.Snmp.SnmpCommunity;
            SnmpAgentIp = currentDatacenterParameters.Snmp.SnmpAgentIp;
            SelectedSnmpEncoding = currentDatacenterParameters.Snmp.SnmpEncoding;
            EnterpriseOid = currentDatacenterParameters.Snmp.EnterpriseOid;
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
                _currentDatacenterParameters.Snmp.IsSnmpOn = IsSnmpOn;
                _currentDatacenterParameters.Snmp.SnmpReceiverIp = SnmpManagerIp;
                _currentDatacenterParameters.Snmp.SnmpReceiverPort = SnmpManagerPort;
                _currentDatacenterParameters.Snmp.SnmpCommunity = SnmpCommunity;
                _currentDatacenterParameters.Snmp.SnmpAgentIp = SnmpAgentIp;
                _currentDatacenterParameters.Snmp.SnmpEncoding = SelectedSnmpEncoding;
                _currentDatacenterParameters.Snmp.EnterpriseOid = EnterpriseOid;

                var dto = new SnmpSettingsDto()
                {
                    IsSnmpOn = IsSnmpOn,
                    SnmpReceiverIp = SnmpManagerIp,
                    SnmpReceiverPort = SnmpManagerPort,
                    SnmpCommunity = SnmpCommunity,
                    SnmpAgentIp = SnmpAgentIp,
                    SnmpEncoding = SelectedSnmpEncoding,
                    EnterpriseOid = EnterpriseOid,
                };
                res = await _c2DWcfManager.SaveAndTestSnmpSettings(dto);
            }

            if (res)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Information, new List<string>()
                {
                    Resources.SID_SNMP_trap_sent,
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

        public void Close()
        {
            TryClose();
        }
    }
}
