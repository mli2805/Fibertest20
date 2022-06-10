using System;
using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class OneMeasurementModel : PropertyChangedBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public CurrentUser CurrentUser;
        public Rtu Rtu;
        public int MeasurementTimeout;

        public OtdrParametersTemplatesViewModel OtdrParametersTemplatesViewModel { get; set; }
        public AutoAnalysisParamsViewModel AutoAnalysisParamsViewModel { get; set; }
        public MeasurementProgressViewModel MeasurementProgressViewModel { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
                OtdrParametersTemplatesViewModel.IsEnabled = _isEnabled;
                AutoAnalysisParamsViewModel.IsEnabled = _isEnabled;
            }
        }

        public ObservableCollection<string> TraceResults { get; set; } = new ObservableCollection<string>();

        private Visibility _traceResultsVisibility = Visibility.Collapsed;
        public Visibility TraceResultsVisibility
        {
            get => _traceResultsVisibility;
            set
            {
                if (value == _traceResultsVisibility) return;
                _traceResultsVisibility = value;
                NotifyOfPropertyChange();
            }
        }
    }
}