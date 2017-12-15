using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceStateVm : PropertyChangedBase
    {
        public Guid TraceId { get; set; }
        public string TraceTitle { get; set; }
        public string RtuTitle { get; set; }
        public string PortTitle { get; set; }

        public FiberState TraceState { get; set; }
        public BaseRefType BaseRefType { get; set; }

        public Brush TraceStateBrush =>
            TraceState == FiberState.Ok
                ? Brushes.White
                : BaseRefType == BaseRefType.Fast
                    ? Brushes.Yellow
                    : TraceState.GetBrush(isForeground: true);

        public string TraceStateOnScreen => BaseRefType == BaseRefType.Fast && TraceState != FiberState.Ok
            ? FiberState.Suspicion.ToLocalizedString()
            : TraceState.ToLocalizedString();

        public EventStatus EventStatus { get; set; } = EventStatus.NotAnAccident;
        public string Comment { get; set; }

        public DateTime MeasurementTimestamp { get; set; }
        public int SorFileId { get; set; }
        public string StateOn => string.Format(Resources.SID_State_on__0_, MeasurementTimestamp.ToString(CultureInfo.CurrentCulture));

        public Visibility OpticalEventPanelVisibility
            => EventStatus > EventStatus.NotAnAccident ? Visibility.Visible : Visibility.Collapsed;

        public Visibility AccidentsPanelVisibility
            => TraceState == FiberState.Ok ? Visibility.Collapsed : Visibility.Visible;

        public bool IsAccidentPlaceButtonEnabled => TraceState != FiberState.Ok;

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
    }
}
