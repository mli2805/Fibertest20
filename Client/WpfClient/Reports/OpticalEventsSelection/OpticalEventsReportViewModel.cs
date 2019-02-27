using System;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsReportViewModel : Screen
    {
        private readonly OpticalEventsReportProvider _opticalEventsReportProvider;
        private bool _isCustomReport;
        public bool IsCurrentEventsReport { get; set; }

        public bool IsCustomReport
        {
            get { return _isCustomReport; }
            set
            {
                if (value == _isCustomReport) return;
                _isCustomReport = value;
                NotifyOfPropertyChange();
            }
        }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public EventStatusViewModel EventStatusViewModel { get; set; } = new EventStatusViewModel();
        public TraceStateSelectionViewModel TraceStateSelectionViewModel { get; set; } = new TraceStateSelectionViewModel();


        public PdfDocument Report { get; set; }

        public OpticalEventsReportViewModel(OpticalEventsReportProvider opticalEventsReportProvider)
        {
            _opticalEventsReportProvider = opticalEventsReportProvider;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Optical_events_report;
            Report = null;
        }

        public void CreateReport()
        {
            Report = _opticalEventsReportProvider.Create();
            TryClose();
        }
    }
}
