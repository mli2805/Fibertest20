using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceStateModelHeader
    {
        public string TraceTitle { get; set; } = "";
        public string RtuTitle { get; set; } = "";
        public string PortTitle { get; set; } = "";
    }
    public class TraceStateModel : PropertyChangedBase
    {
        public Guid TraceId { get; set; }
        public TraceStateModelHeader Header { get; set; }
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

        public EventStatus EventStatus { get; set; } = EventStatus.EventButNotAnAccident;
        public string Comment { get; set; }

        public DateTime MeasurementTimestamp { get; set; }
        public int SorFileId { get; set; }
        public string StateOn => string.Format(Resources.SID_State_on__0_, MeasurementTimestamp.ToString(CultureInfo.CurrentCulture));

        public Visibility OpticalEventPanelVisibility
            => EventStatus > EventStatus.EventButNotAnAccident ? Visibility.Visible : Visibility.Collapsed;

        public List<AccidentLineModel> Accidents { get; set; } = new List<AccidentLineModel>();

       
        public string SelectedAccidentGpsCoordinates { get; set; }

        public Visibility AccidentsPanelVisibility
            => TraceState == FiberState.Ok || TraceState == FiberState.NoFiber ? Visibility.Collapsed : Visibility.Visible;

        public string AccidentsHeader => string.Format(Resources.SID_Accidents_count___0_, Accidents.Count);

        public bool IsAccidentPlaceButtonEnabled => TraceState != FiberState.Ok;

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
    }
}
