using System;
using System.Threading;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class PortLineModel : PropertyChangedBase
    {
        public string Number { get; set; }
        public Guid TraceId { get; set; } = Guid.Empty;
        public string TraceTitle { get; set; }

        private FiberState _traceState;
        public FiberState TraceState
        {
            get { return _traceState; }
            set
            {
                if (value == _traceState) return;
                _traceState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TraceStateOnScreen));
                NotifyOfPropertyChange(nameof(TraceStateBrush));
            }
        }

        public string TraceStateOnScreen => TraceId == Guid.Empty ? "" : TraceState.ToLocalizedString();
        public Brush TraceStateBrush => TraceState.GetBrush(false);

        private DateTime? _timestamp;
        public DateTime? Timestamp
        {
            get { return _timestamp; }
            set
            {
                if (value.Equals(_timestamp)) return;
                _timestamp = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TimestampOnScreen));
            }
        }

        public string TimestampOnScreen => Timestamp?.ToString(Thread.CurrentThread.CurrentUICulture) ?? "";
    }
}