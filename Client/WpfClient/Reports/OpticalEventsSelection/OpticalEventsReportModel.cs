using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsReportModel : PropertyChangedBase
    {
        private bool _isCustomReport;
        public bool IsCustomReport
        {
            get => _isCustomReport;
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

        public Visibility ZoneSelectionVisibility { get; set; }
        public List<Zone> Zones { get; set; }
        public Zone SelectedZone { get; set; }

        private bool _isDetailedReport;

        public bool IsDetailedReport
        {
            get => _isDetailedReport;
            set
            {
                if (value == _isDetailedReport) return;
                _isDetailedReport = value;
                NotifyOfPropertyChange();
                IsAccidentPlaceShown = false;
            }
        }

        private bool _isAccidentPlaceShown;
        public bool IsAccidentPlaceShown
        {
            get { return _isAccidentPlaceShown; }
            set
            {
                if (value == _isAccidentPlaceShown) return;
                _isAccidentPlaceShown = value;
                NotifyOfPropertyChange();
            }
        }
    }
}