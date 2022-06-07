using System;
using Autofac;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel
    {
        public void LaunchComponentsReport()
        {
            _componentsReportViewModel.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(_componentsReportViewModel);
            if (_componentsReportViewModel.Report == null) return;

            PdfExposer.Show(_componentsReportViewModel.Report, 
                $@"MonitoringSystemComponentsReport{DateTime.Now:yyyyMMddHHmmss}.pdf",
                _windowManager);
        }

        public void LaunchOpticalEventsReport()
        {
            _opticalEventsReportViewModel.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(_opticalEventsReportViewModel);
            if (_opticalEventsReportViewModel.Report == null) return;

            PdfExposer.Show(_opticalEventsReportViewModel.Report,
                $@"OpticalEventsReport{DateTime.Now:yyyyMMddHHmmss}.pdf",
                _windowManager);
        }

        public void LaunchEventLogView()
        {
            var vm = _globalScope.Resolve<EventLogViewModel>();
            vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}
