using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewModel : Screen
    {

        public TraceStateVm Model { get; set; }

        public List<EventStatusComboItem> StatusRows { get; set; } = new List<EventStatusComboItem>();
        public EventStatusComboItem SelectedEventStatus { get; set; }

        public TraceStateViewModel()
        {

        }

        public void Initialize(TraceStateVm model)
        {
            Model = model;
            if (Model.EventStatus != EventStatus.NotAnAccident)
                InitializeEventStatusCombobox();
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace_state;
        }

        private void InitializeEventStatusCombobox()
        {
            foreach (var eventStatus in Enum.GetValues(typeof(EventStatus)).OfType<EventStatus>())
            {
                StatusRows.Add(new EventStatusComboItem() {EventStatus = eventStatus});
            }
            SelectedEventStatus = StatusRows.First(r=>r.EventStatus == Model.EventStatus);
        }
    }

    public class EventStatusComboItem
    {
        public EventStatus EventStatus { get; set; }

        public override string ToString()
        {
            return EventStatus.GetLocalizedString();
        }
    }
}
