using System.Collections.Generic;
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
        private Visibility _opticalEventsVisibility;

        public Visibility OpticalEventsVisibility
        {
            get { return _opticalEventsVisibility; }
            set
            {
                if (value == _opticalEventsVisibility) return;
                _opticalEventsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public List<OpticalEventVm> CurrentEventsRows => Rows.Where(e => e.TraceState != FiberState.Ok).ToList();

        public ObservableCollection<OpticalEventVm> Rows { get; set; } = new ObservableCollection<OpticalEventVm>();
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
                        ? Brushes.White 
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
