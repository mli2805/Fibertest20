using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    
    public class DbOptimizationViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public DbOptimizationModel Model { get; set; } = new DbOptimizationModel();

        public DbOptimizationViewModel(IMyLog logFile, Model readModel, IWcfServiceForClient c2DWcfManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
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

    }
}
