using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class DbOptimizationViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public DbOptimizationModel Model { get; set; } = new DbOptimizationModel();

        public DbOptimizationViewModel(IniFile iniFile, IMyLog logFile, Model readModel, 
            CurrentUser currentUser, CurrentDatacenterParameters currentDatacenterParameters,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _readModel = readModel;
            _currentUser = currentUser;
            _currentDatacenterParameters = currentDatacenterParameters;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        public async void Initialize()
        {
            var drive = await _c2DWcfManager.GetDiskSpaceGb();
            if (drive == null)
            {
                _logFile.AppendLine(@"GetDiskSpaceGb error");
                return;
            }
            Model.DriveSize = $@"{drive.TotalSize:0.0} Gb";
            Model.DataSize = $@"{drive.DataSize:0.000} Gb";
            Model.AvailableFreeSpace = $@"{drive.AvailableFreeSpace:0.0} Gb";
            Model.FreeSpaceThreshold = $@"{drive.FreeSpaceThreshold:0.0} Gb";

            Model.OpticalEvents = _readModel.Measurements.Count(m => m.EventStatus > EventStatus.JustMeasurementNotAnEvent);
            Model.MeasurementsNotEvents = _readModel.Measurements.Count(m => m.EventStatus == EventStatus.JustMeasurementNotAnEvent);
            Model.NetworkEvents = _readModel.NetworkEvents.Count + _readModel.BopNetworkEvents.Count;

            var flag = _iniFile.Read(IniSection.MySql, IniKey.IsOptimizationCouldBeDoneUpToToday, false);
            Model.UpToLimit = flag ? DateTime.Today : new DateTime(DateTime.Today.Year - 2, 12, 31);
            Model.SelectedDate = flag ? DateTime.Today : new DateTime(DateTime.Today.Year - 2, 12, 31);

            var daysForEventLog = _iniFile.Read(IniSection.MySql, IniKey.SnapshotUptoLimitInDays, 90);
            Model.FromLimit2 = _currentDatacenterParameters.SnapshotLastDate.AddDays(1);
            Model.UpToLimit2 = DateTime.Today.Date.AddDays(-daysForEventLog);
            Model.SelectedDate2 = DateTime.Today.Date.AddDays(-daysForEventLog);

            Model.IsEnabled = _currentUser.Role <= Role.Root;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Database_optimization;
        }

        public async void Execute()
        {
            if (!Validate()) return;

            var cmd = Model.IsRemoveMode ?
                (object)new RemoveEventsAndSors()
                {
                    IsMeasurementsNotEvents = Model.IsMeasurements,
                    IsOpticalEvents = Model.IsOpticalEvents,
                    IsNetworkEvents = Model.IsNetworkEvents,
                    UpTo = Model.SelectedDate,
                }
                : new MakeSnapshot()
                {
                    UpTo = Model.SelectedDate2,
                };
            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (!string.IsNullOrEmpty(result))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        private bool Validate()
        {
            if (Model.IsRemoveMode && !Model.IsMeasurements && !Model.IsOpticalEvents && !Model.IsNetworkEvents)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Nothing_selected_to_remove_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            var vm2 = new MyMessageBoxViewModel(MessageType.Confirmation,
                new List<string>
                {
                    Resources.SID_Attention_, "",
                    Resources.SID_If_you_click_OK_now__the_data_will_be_permanently_deleted,
                    Resources.SID_with_no_possibility_to_restore_them_,
                }, 0);
            _windowManager.ShowDialogWithAssignedOwner(vm2);
            return vm2.IsAnswerPositive;
        }

    }
}
