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
    public class TraceStateModelHeader : PropertyChangedBase
    {
        private string _traceTitle = "";
        private string _rtuTitle = "";

        public string TraceTitle
        {
            get => _traceTitle;
            set
            {
                if (value == _traceTitle) return;
                _traceTitle = value;
                NotifyOfPropertyChange();
            }
        }

        public string RtuTitle
        {
            get => _rtuTitle;
            set
            {
                if (value == _rtuTitle) return;
                _rtuTitle = value;
                NotifyOfPropertyChange();
            }
        }

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

        private string PrepareTraceStateOnScreen ()
        {
            if (TraceState == FiberState.Ok) return @"OK";
            if (BaseRefType == BaseRefType.Fast)
            {
                return TraceState != FiberState.NoFiber
                    ? FiberState.Suspicion.ToLocalizedString()
                    : $@"{FiberState.Suspicion.ToLocalizedString()}  ({FiberState.NoFiber.ToLocalizedString()})";
            }
            return TraceState.ToLocalizedString();
        }

        public string TraceStateOnScreen => PrepareTraceStateOnScreen();

        public EventStatus EventStatus { get; set; } = EventStatus.EventButNotAnAccident;
        public string Comment { get; set; }

        public DateTime MeasurementTimestamp { get; set; }
        public int SorFileId { get; set; }
        public string StateOn => string.Format(Resources.SID_State_on__0_, MeasurementTimestamp.ToString(CultureInfo.CurrentCulture), SorFileId);

        public Visibility OpticalEventPanelVisibility
            => EventStatus > EventStatus.EventButNotAnAccident ? Visibility.Visible : Visibility.Collapsed;

        public List<AccidentLineModel> Accidents { get; set; } = new List<AccidentLineModel>();

       
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
