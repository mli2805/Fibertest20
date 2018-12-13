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
    public class RtuChannelViewModel : Screen
    {
        private readonly SoundManager _soundManager;
        private readonly Model _readModel;
        public string RtuTitle { get; set; }
        public string RtuAvailability { get; set; }
        public string ChannelName { get; set; }
        public string StateOn { get; set; }
        public string ChannelState { get; set; }
        public Brush ChannelStateBrush { get; set; }
    
        public bool IsOk { get; set; }

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

        public RtuChannelViewModel(SoundManager soundManager, Model readModel)
        {
            _soundManager = soundManager;
            _readModel = readModel;
        }

        public void Initialize(NetworkEventAdded networkEventAdded)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == networkEventAdded.RtuId);
            RtuTitle = rtu.Title;
            RtuAvailability = rtu.IsAvailable ? Resources.SID_RTU_is_available : Resources.SID_RTU_is_not_available;
            ChannelName = Resources.SID_Main;
            StateOn = string.Format(Resources.SID_State_on__0_,
                networkEventAdded.EventTimestamp.ToString(CultureInfo.CurrentCulture), networkEventAdded.Ordinal);
            IsOk = networkEventAdded.RtuPartStateChanges == RtuPartStateChanges.OnlyBetter;
            ChannelState = IsOk ? Resources.SID_Ok : Resources.SID_Breakdown;
            ChannelStateBrush = IsOk ? Brushes.White : Brushes.Red;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_RTU_connection_state;
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
