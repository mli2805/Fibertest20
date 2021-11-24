using System;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class BopStateViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly SoundManager _soundManager;
        private readonly Model _readModel;
        public Guid BopId { get; set; }
        public string BopIp { get; set; }
        public string PortRtu { get; set; }
        public string RtuTitle { get; set; }
        public string ServerTitle { get; set; }
        public string StateOn { get; set; }
        public string BopState { get; set; }
        public Brush BopStateBrush { get; set; }
        public bool IsOk { get; set; }

        private bool _isSoundForThisVmInstanceOn;
        private bool _isSoundButtonEnabled;
        public bool IsSoundButtonEnabled
        {
            get => _isSoundButtonEnabled;
            set
            {
                if (value == _isSoundButtonEnabled) return;
                _isSoundButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsOpen { get; private set; }

        public BopStateViewModel(CurrentDatacenterParameters currentDatacenterParameters, SoundManager soundManager, Model readModel)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _soundManager = soundManager;
            _readModel = readModel;
        }

        public void Initialize(BopNetworkEventAdded bopNetworkEventAdded)
        {
            var otau = _readModel.Otaus.FirstOrDefault(o =>
                o.NetAddress.Ip4Address == bopNetworkEventAdded.OtauIp &&
                o.NetAddress.Port == bopNetworkEventAdded.TcpPort);
            if (otau == null) return;

            BopId = otau.Id;
            PortRtu = otau.MasterPort != 0 ? otau.MasterPort.ToString() : "";
            BopIp = !string.IsNullOrEmpty(otau.VeexRtuMainOtauId) && otau.VeexRtuMainOtauId.StartsWith(@"S1_") 
                ? Resources.SID_Main : bopNetworkEventAdded.OtauIp;

            var rtu = _readModel.Rtus.First(r => r.Id == bopNetworkEventAdded.RtuId);
            RtuTitle = rtu.Title;
            ServerTitle = _currentDatacenterParameters.ServerTitle;
            StateOn = string.Format(Resources.SID_State_at_,
                bopNetworkEventAdded.EventTimestamp.ToString(CultureInfo.CurrentCulture), bopNetworkEventAdded.Ordinal);
            IsOk = bopNetworkEventAdded.IsOk;
            BopState = bopNetworkEventAdded.IsOk ? Resources.SID_OK_BOP : Resources.SID_Bop_breakdown;
            BopStateBrush = bopNetworkEventAdded.IsOk ? Brushes.White : Brushes.Red;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_BOP_state;
            IsOpen = true;

            if (IsOk)
                _soundManager.PlayOk();
            else
            {
                _soundManager.StartAlert();
                IsSoundButtonEnabled = true;
                _isSoundForThisVmInstanceOn = true;
            }
        }

        public void TurnSoundOff()
        {
            if (_isSoundForThisVmInstanceOn)
            {
                _isSoundForThisVmInstanceOn = false;
                _soundManager.StopAlert();
                IsSoundButtonEnabled = false;
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            IsOpen = false;
            callback(true);
        }
    }
}
