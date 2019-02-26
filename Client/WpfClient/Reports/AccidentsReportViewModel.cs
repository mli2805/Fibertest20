using Caliburn.Micro;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public class AccidentsReportViewModel : Screen
    {
        private readonly AccidentsReportProvider _accidentsReportProvider;
        public bool IsCurrentAccidentsReport { get; set; }

        public bool IsCustomReport { get; set; }

        public PdfDocument Report { get; set; }

        public AccidentsReportViewModel(AccidentsReportProvider accidentsReportProvider)
        {
            _accidentsReportProvider = accidentsReportProvider;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Choose accidents report parameters";
            Report = null;
        }

        public void CreateReport()
        {
            Report = _accidentsReportProvider.Create();
            TryClose();
        }
    }
}
