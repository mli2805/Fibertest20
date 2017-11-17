using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
        private string _selectedState;

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

        public List<string> StateList { get; set; }

        public string SelectedState
        {
            get { return _selectedState; }
            set
            {
                if (value == _selectedState) return;
                _selectedState = value;
                NotifyOfPropertyChange();
            }
        }

        public OpticalEventsViewModel(ReadModel readModel)
        {
            _readModel = readModel;
            InitializeStateList();
        }

        private void InitializeStateList()
        {
            StateList = new List<string>()
            {
                "All",
                FiberState.Ok.GetLocalizedString(), 
                FiberState.Minor.GetLocalizedString(),
                FiberState.Major.GetLocalizedString(),
                FiberState.Critical.GetLocalizedString(),
                FiberState.FiberBreak.GetLocalizedString(),
                FiberState.NoFiber.GetLocalizedString(),
                FiberState.User.GetLocalizedString(),
            };
            SelectedState = StateList.First();
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
                            ? Brushes.Yellow : opticalEvent.TraceState.GetBrush(),
                TraceState = opticalEvent.TraceState,

                EventStatus = 
                    opticalEvent.IsStatusAcceptable()
                        ? opticalEvent.EventStatus.GetLocalizedString() 
                        : "",
                EventStatusBrush = opticalEvent.EventStatus == EventStatus.Confirmed ? Brushes.Red : Brushes.White,
                StatusTimestamp = opticalEvent.IsStatusAcceptable() ? opticalEvent.StatusTimestamp.ToString(Thread.CurrentThread.CurrentUICulture) : "",
                StatusUsername = opticalEvent.IsStatusAcceptable() ? opticalEvent.StatusUserId.ToString() : "",
                Comment = opticalEvent.Comment,
            });
        }


    }
}
