﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly ComponentsReportProvider _componentsReportProvider;
        private readonly OpticalEventsReportViewModel _opticalEventsReportViewModel;


        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager,
            ComponentsReportProvider componentsReportProvider, OpticalEventsReportViewModel opticalEventsReportViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _componentsReportProvider = componentsReportProvider;
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


        public void LaunchComponentsReport()
        {
            var report = _componentsReportProvider.Create();
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string filename = Path.Combine(folder, @"MonitoringSystemComponentsReport.pdf");
                report.Save(filename);
                Process.Start(filename);
            }
            catch (Exception e)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, e.Message);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        public void LaunchOpticalEventsReport()
        {
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
                var vm = new MyMessageBoxViewModel(MessageType.Error, e.Message);
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

        public async void LaunchEventLogView()
        {
            var vm = _globalScope.Resolve<EventLogViewModel>();
            await vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void ShowUsersGuide()
        {
            var usersGuide = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"..\UserGuide\FIBERTEST20ClientUGru.pdf"));
            if (!File.Exists(usersGuide))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error,
                    new List<string> {Resources.SID_Cannot_find_file_with_user_s_guide_, "", usersGuide}, 0);
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return;
            }
            Process.Start(usersGuide);
        }

        public void LaunchLicenseView()
        {
            var vm = _globalScope.Resolve<LicenseViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchAboutView()
        {
            var vm = _globalScope.Resolve<AboutViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}