using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
        }

        public void Apply(OpticalEvent opticalEvent)
        {
            Rows.Add(new OpticalEventVm()
            {
                Nomer = opticalEvent.Id,
                EventTimestamp = opticalEvent.EventTimestamp,
                RtuTitle = _readModel.Rtus.FirstOrDefault(r=>r.Id == opticalEvent.RtuId)?.Title,
                TraceTitle = _readModel.Traces.FirstOrDefault(t=>t.Id == opticalEvent.TraceId)?.Title,
                BaseRefTypeBrush = 
                    opticalEvent.TraceState == FiberState.Ok 
                        ? Brushes.LightGreen 
                        : opticalEvent.BaseRefType == BaseRefType.Fast 
                            ? Brushes.Yellow : Brushes.LightPink,
                TraceState = opticalEvent.TraceState,

                EventStatus = 
                    opticalEvent.IsStatusAcceptable()
                        ? opticalEvent.EventStatus.ToString() 
                        : "",
                StatusTimestamp = opticalEvent.IsStatusAcceptable() ? opticalEvent.StatusTimestamp.ToString() : "",
                StatusUsername = opticalEvent.IsStatusAcceptable() ? opticalEvent.StatusUserId.ToString() : "",
                Comment = opticalEvent.Comment,
            });
        }


    }
}
