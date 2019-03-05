using System;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsReportViewModel : Screen
    {
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly ActualOpticalEventsReportProvider _actualOpticalEventsReportProvider;
        private readonly AllOpticalEventsReportProvider _allOpticalEventsReportProvider;

        public OpticalEventsReportModel Model { get; set; } = new OpticalEventsReportModel();
        public PdfDocument Report { get; set; }

        public OpticalEventsReportViewModel(CurrentUser currentUser, Model readModel,
            ActualOpticalEventsReportProvider actualOpticalEventsReportProvider, 
            AllOpticalEventsReportProvider allOpticalEventsReportProvider)
        {
            _currentUser = currentUser;
            _readModel = readModel;
            _actualOpticalEventsReportProvider = actualOpticalEventsReportProvider;
            _allOpticalEventsReportProvider = allOpticalEventsReportProvider;
        }

        public void Initialize()
        {
            Model.ZoneSelectionVisibility = _currentUser.ZoneId == Guid.Empty ? Visibility.Visible : Visibility.Collapsed;
            Model.Zones = _readModel.Zones;
            Model.SelectedZone = Model.Zones.First();

            Model.DateTo = DateTime.Now;
            Model.DateFrom = new DateTime(DateTime.Today.Year, 1, 1);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Optical_events_report;
            Report = null;
        }

        public void CreateReport()
        {
            Report =  Model.IsCustomReport 
                ? _allOpticalEventsReportProvider.Create(Model) 
                : _actualOpticalEventsReportProvider.Create(Model);
            TryClose();
        }
    }
}
