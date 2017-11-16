using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsViewModel : PropertyChangedBase
    {
        private readonly ReadModel _readModel;
        private Visibility _opticalEventsVisiblility;

        public Visibility OpticalEventsVisiblility
        {
            get { return _opticalEventsVisiblility; }
            set
            {
                if (value == _opticalEventsVisiblility) return;
                _opticalEventsVisiblility = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<OpticalEventVm> _rows = new ObservableCollection<OpticalEventVm>();
        public ObservableCollection<OpticalEventVm> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        public OpticalEventsViewModel(ReadModel readModel)
        {
            _readModel = readModel;
            Rows.Add(new OpticalEventVm() {Nomer = 1, RtuTitle = @"just for test"});
        }

        public void Apply(OpticalEvent newOpticalEvent)
        {
            Rows.Add(new OpticalEventVm()
            {
                Nomer = newOpticalEvent.Id,
                EventTimestamp = newOpticalEvent.EventTimestamp,
                RtuTitle = _readModel.Rtus.FirstOrDefault(r=>r.Id == newOpticalEvent.RtuId)?.Title,
                TraceTitle = _readModel.Traces.FirstOrDefault(t=>t.Id == newOpticalEvent.TraceId)?.Title,
                TraceState = newOpticalEvent.TraceState,
                EventStatus = newOpticalEvent.EventStatus,
                StatusTimestamp = newOpticalEvent.StatusTimestamp,
                StatusUsername = newOpticalEvent.StatusUserId.ToString(),
            });
        }
    }
}
