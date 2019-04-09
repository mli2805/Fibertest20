using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    
    public class DbOptimizationViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public DbOptimizationModel Model { get; set; } = new DbOptimizationModel();

        public DbOptimizationViewModel(IMyLog logFile, Model readModel, 
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _logFile = logFile;
            _readModel = readModel;
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
            Model.DriveSize = $@"{drive.TotalSize:#.0} Gb" ;
            Model.AvailableFreeSpace = $@"{drive.AvailableFreeSpace:#.0} Gb";
            Model.FreeSpaceThreshold = $@"{drive.FreeSpaceThreshold:#.0} Gb";

            Model.OpticalEvents = _readModel.Measurements.Count(m => m.EventStatus > EventStatus.JustMeasurementNotAnEvent);
            Model.MeasurementsNotEvents = _readModel.Measurements.Count(m => m.EventStatus == EventStatus.JustMeasurementNotAnEvent);
            Model.NetworkEvents = _readModel.NetworkEvents.Count + _readModel.BopNetworkEvents.Count;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Database_optimization;
        }

        public async void Execute()
        {
            if (!Validate()) return;

            var cmd = new StartDbOptimazation()
            {
                IsRemoveElementsMode = Model.IsRemoveMode,
                IsMeasurementsNotEvents = Model.IsMeasurements,
                IsOpticalEvents = Model.IsOpticalEvents,
                IsNetworkEvents = Model.IsNetworkEvents,
                UpTo = Model.SelectedDate,
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
