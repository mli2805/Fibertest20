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
        private readonly ReflectogramManager _reflectogramManager;
        public TraceStateVm Model { get; set; }

        public List<EventStatusComboItem> StatusRows { get; set; }
        public EventStatusComboItem SelectedEventStatus { get; set; }

        public TraceStateViewModel(ReflectogramManager reflectogramManager)
        {
            _reflectogramManager = reflectogramManager;
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
            StatusRows = new List<EventStatusComboItem>
            {  // not foreach because order matters
                new EventStatusComboItem() {EventStatus = EventStatus.Confirmed},
                new EventStatusComboItem() {EventStatus = EventStatus.NotConfirmed},
                new EventStatusComboItem() {EventStatus = EventStatus.Planned},
                new EventStatusComboItem() {EventStatus = EventStatus.Suspended},
                new EventStatusComboItem() {EventStatus = EventStatus.Unprocessed}
            };

            SelectedEventStatus = StatusRows.First(r=>r.EventStatus == Model.EventStatus);
        }


        //----

        public void ShowAccidentPlace() { }
        public void ShowReflectogram() { _reflectogramManager.ShowRefWithBase(Model.SorFileId); }
        public void ShowRftsEvents() { }
        public void ShowTraceStatistics() { }
        public void ExportToKml() { }
        public void ShowReport() { }
    }
}
