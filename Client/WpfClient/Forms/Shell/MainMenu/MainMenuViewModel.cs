using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;
using Microsoft.Win32;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly ComponentsReportViewModel _componentsReportViewModel;
        private readonly OpticalEventsReportViewModel _opticalEventsReportViewModel;
 
        private Role _currentUserRole = Role.Supervisor;
        public Role CurrentUserRole
        {
            get => _currentUserRole;
            set
            {
                if (Equals(value, _currentUserRole)) return;
                _currentUserRole = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(DeveloperMenuItemsVisibility));
            }
        }

        public Visibility DeveloperMenuItemsVisibility =>
            _currentUserRole == Role.Developer ? Visibility.Visible : Visibility.Collapsed;

        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager, IMyLog logFile, Model readModel,
            ComponentsReportViewModel componentsReportViewModel, OpticalEventsReportViewModel opticalEventsReportViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _logFile = logFile;
            _readModel = readModel;
            _componentsReportViewModel = componentsReportViewModel;
            _opticalEventsReportViewModel = opticalEventsReportViewModel;

        }

        public void LaunchResponsibilityZonesView()
        {
            var vm = _globalScope.Resolve<ZonesViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchUserListView()
        {
            var vm = _globalScope.Resolve<UserListViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchObjectsToZonesView()
        {
            var vm = _globalScope.Resolve<ObjectsAsTreeToZonesViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void ImportRtuFromFolder()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var folder = Path.GetFullPath(Path.Combine(basePath, @"..\temp\"));
            string[] files = Directory.GetFiles(folder, "*.brtu");

            foreach (var filename in files)
            {
                var bytes = File.ReadAllBytes(filename);
                var oneRtuGraphModel = new Model();
                if (!await oneRtuGraphModel.Deserialize(_logFile, bytes)) return;
                _readModel.AddOneRtuToModel(oneRtuGraphModel);
            }
        }


        public void LaunchComponentsReport()
        {
            _componentsReportViewModel.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(_componentsReportViewModel);
            if (_componentsReportViewModel.Report == null) return;
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string filename = Path.Combine(folder, @"MonitoringSystemComponentsReport.pdf");
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

                string filename = Path.Combine(folder, @"OpticalEventsReport.pdf");
                _opticalEventsReportViewModel.Report.Save(filename);
                Process.Start(filename);
            }
            catch (Exception e)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, @"LaunchOpticalEventsReport: " + e.Message);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        public void LaunchSmtpSettingsView()
        {
            var vm = _globalScope.Resolve<SmtpSettingsViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchSmsSettingsView()
        {
            var vm = _globalScope.Resolve<SmsSettingsViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
        public void LaunchSnmpSettingsView()
        {
            var vm = _globalScope.Resolve<SnmpSettingsViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchClientSettingsView()
        {
            var vm = _globalScope.Resolve<ConfigurationViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchGisSettingsView()
        {
            var vm = _globalScope.Resolve<GisSettingsViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchCleaningView()
        {
            var vm = _globalScope.Resolve<DbOptimizationViewModel>();
            vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void LaunchEventLogView()
        {
            var vm = _globalScope.Resolve<EventLogViewModel>();

            var waitVm = new WaitViewModel();
            waitVm.Initialize(LongOperation.CollectingEventLog);
            _windowManager.ShowWindowWithAssignedOwner(waitVm);

            var unused = await Task.Factory.StartNew(()=>vm.Initialize().Result);

            waitVm.TryClose();

            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void ShowUsersGuide()
        {
            var usersGuide = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"..\UserGuide\FIBERTEST20ClientUGru.pdf"));
            if (!File.Exists(usersGuide))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error,
                    new List<string> { Resources.SID_Cannot_find_file_with_user_s_guide_, "", usersGuide }, 0);
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return;
            }
            Process.Start(usersGuide);
        }

        public void LaunchLicenseView()
        {
            var vm = _globalScope.Resolve<LicenseViewModel>();
            vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchAboutView()
        {
            var vm = _globalScope.Resolve<AboutViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}