using System;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class BopStateViewModel : Screen
    {
        private readonly SoundManager _soundManager;
        private readonly Model _readModel;
        public string BopIp { get; set; }
        public int PortRtu { get;set; }
        public string RtuTitle { get; set; }
        public string StateOn { get; set; }
        public string BopState { get; set; }
        public Brush BopStateBrush { get; set; }
        public bool IsOk { get; set; }

        private bool _isSoundButtonEnabled;
        public bool IsSoundButtonEnabled
        {
            get { return _isSoundButtonEnabled; }
            set
            {
                if (value == _isSoundButtonEnabled) return;
                _isSoundButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsOpen { get; private set; }

        public BopStateViewModel(SoundManager soundManager, Model readModel)
        {
            _soundManager = soundManager;
            _readModel = readModel;
        }

        public void Initialize(BopNetworkEventAdded bopNetworkEventAdded)
        {
            BopIp = bopNetworkEventAdded.OtauIp;
            var otau = _readModel.Otaus.FirstOrDefault(o =>
                o.NetAddress.Ip4Address == bopNetworkEventAdded.OtauIp &&
                o.NetAddress.Port == bopNetworkEventAdded.TcpPort);
            if (otau != null)
                PortRtu = otau.MasterPort;

            RtuTitle = _readModel.Rtus.First(r => r.Id == bopNetworkEventAdded.RtuId).Title;
            StateOn = string.Format(Resources.SID_State_on__0_,
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
            }
        }

        public void TurnSoundOff()
        {
            if (IsSoundButtonEnabled)
            {
                _soundManager.StopAlert();
                IsSoundButtonEnabled = false;
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            if (IsSoundButtonEnabled)
                _soundManager.StopAlert();
            IsOpen = false;
            callback(true);
        }
    }
}
