using System;
using System.Diagnostics;
using System.IO;
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
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string filename = Path.Combine(folder, $@"MonitoringSystemComponentsReport{DateTime.Now:yyyyMMddHHmmss}.pdf");
                _componentsReportViewModel.Report.Save(filename);
                Process.Start(filename);
            }
            catch (Exception e)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, @"LaunchComponentsReport: " + e.Message);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        public void LaunchOpticalEventsReport()
        {
            _opticalEventsReportViewModel.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(_opticalEventsReportViewModel);
            if (_opticalEventsReportViewModel.Report == null) return;

            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string filename = Path.Combine(folder, $@"OpticalEventsReport{DateTime.Now:yyyyMMddHHmmss}.pdf");
                _opticalEventsReportViewModel.Report.Save(filename);
                Process.Start(filename);
            }
            catch (Exception e)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, @"LaunchOpticalEventsReport: " + e.Message);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        public void LaunchEventLogView()
        {
            var vm = _globalScope.Resolve<EventLogViewModel>();
            vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}
