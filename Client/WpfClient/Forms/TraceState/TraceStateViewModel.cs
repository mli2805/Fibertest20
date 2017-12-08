using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewModel : Screen
    {
        private readonly ReflectogramManager _reflectogramManager;
        private readonly SoundManager _soundManager;
        public TraceStateVm Model { get; set; }
        public bool IsLastStateForThisTrace { get; set; } 

        public List<EventStatusComboItem> StatusRows { get; set; }
        public EventStatusComboItem SelectedEventStatus { get; set; }

        public TraceStateViewModel(ReflectogramManager reflectogramManager, SoundManager soundManager)
        {
            _reflectogramManager = reflectogramManager;
            _soundManager = soundManager;
        }

        public void Initialize(TraceStateVm model, bool isLastStateForThisTrace)
        {
            Model = model;
            IsLastStateForThisTrace = isLastStateForThisTrace;
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

            SelectedEventStatus = StatusRows.FirstOrDefault(r=>r.EventStatus == Model.EventStatus);
        }


        //----
        public void TurnSound()
        {
            _soundManager.PlayOk();
        }

        public void ShowAccidentPlace() { }
        public void ShowReflectogram() { _reflectogramManager.ShowRefWithBase(Model.SorFileId); }
        public void ShowRftsEvents() { }
        public void ShowTraceStatistics() { }
        public void ExportToKml() { }
        public void ShowReport() { }
    }
}
