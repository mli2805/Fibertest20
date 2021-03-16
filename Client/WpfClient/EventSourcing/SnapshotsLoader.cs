using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class SnapshotsLoader
    {
        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly Model _readModel;
        private readonly ZoneEventsOnTreeExecutor _zoneEventsOnTreeExecutor;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;

        public SnapshotsLoader(IMyLog logFile, ILocalDbManager localDbManager,
            CurrentDatacenterParameters currentDatacenterParameters, IWcfServiceDesktopC2D c2DWcfManager,
            Model readModel, ZoneEventsOnTreeExecutor zoneEventsOnTreeExecutor,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel)
        {
            _logFile = logFile;
            _localDbManager = localDbManager;
            _currentDatacenterParameters = currentDatacenterParameters;
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _zoneEventsOnTreeExecutor = zoneEventsOnTreeExecutor;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
        }

        public async Task<int> LoadAndApplySnapshot(bool isCacheCleared)
        {
            if (_currentDatacenterParameters.SnapshotLastEvent == 0)
                return 0;
            _logFile.AppendLine($@"Loading snapshot ({_currentDatacenterParameters.SnapshotLastEvent})");

            var snapshot = isCacheCleared 
                ? await DownloadSnapshot()
                : await _localDbManager.LoadSnapshot(_currentDatacenterParameters.SnapshotLastEvent);

            if (snapshot == null) return -1;
            var appliedSuccessfully = await ApplySnapshot(snapshot);

            return appliedSuccessfully ? _currentDatacenterParameters.SnapshotLastEvent : -1;
        }

        private async Task<bool> ApplySnapshot(byte[] snapshot)
        {
            if (!await _readModel.Deserialize(_logFile, snapshot))
                return false;
            _zoneEventsOnTreeExecutor.RenderOfModelAfterSnapshot();
            _opticalEventsDoubleViewModel.RenderMeasurementsFromSnapshot();
            _networkEventsDoubleViewModel.RenderNetworkEvents();
            _bopNetworkEventsDoubleViewModel.RenderBopNetworkEvents();
            return true;
        }

        private async Task<byte[]> DownloadSnapshot()
        {
            try
            {
                _logFile.AppendLine(@"Downloading snapshot...");
                var dto = await _c2DWcfManager.GetSnapshotParams(
                    new GetSnapshotDto() { LastIncludedEvent = _currentDatacenterParameters.SnapshotLastEvent });
                _logFile.AppendLine($@"{dto.PortionsCount} portions in snapshot");
                if (dto.PortionsCount < 1)
                    return null;

                var snapshot = new byte[dto.Size];
                var offset = 0;
                for (int i = 0; i < dto.PortionsCount; i++)
                {
                    var portion = await _c2DWcfManager.GetSnapshotPortion(i);
                    var unused = await _localDbManager.SaveSnapshot(portion);
                    portion.CopyTo(snapshot, offset);
                    offset = offset + portion.Length;
                    _logFile.AppendLine($@"portion {i}  {portion.Length} bytes received and saved in cache");
                }

                return snapshot;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"DownloadSnapshot : {e.Message}");
                return null;
            }
        }

    }
}