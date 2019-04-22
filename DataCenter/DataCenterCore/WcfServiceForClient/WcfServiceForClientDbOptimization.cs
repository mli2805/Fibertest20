using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceForClient
    {
        private async Task<string> RemoveEventsAndSors(RemoveEventsAndSors removeEventsAndSors, string username, string clientIp)
        {
            _logFile.AppendLine("Start DB optimization");
            await _d2CWcfManager.BlockClientWhileDbOptimization();
            // block RTUs messages (MSMQ and notifications)
            // block new client's attempts to register
            _globalState.IsDatacenterInDbOptimizationMode = true;

            _logFile.AppendLine("point 1");
            var unused = await ClearSor(removeEventsAndSors);
            // remove from writeModel 
            await _eventStoreService.SendCommand(removeEventsAndSors, username, clientIp);

            // unblock connections
            _globalState.IsDatacenterInDbOptimizationMode = false;
            await _d2CWcfManager.UnBlockClientAfterDbOptimization();
            return "";
        }

        private async Task<int> ClearSor(RemoveEventsAndSors cmd)
        {
            _logFile.AppendLine("point 11");
            if (!cmd.IsMeasurementsNotEvents && !cmd.IsOpticalEvents) return 0;
            var dir = _eventStoreInitializer.DataDir;
            var sorFileInfo = new FileInfo(dir + "sorfiles.ibd");
            var oldSize = sorFileInfo.Length;

            _logFile.AppendLine("Start SorFiles cleaning");
            var ids = _writeModel.GetMeasurementsForDeletion(cmd.UpTo, cmd.IsMeasurementsNotEvents, cmd.IsOpticalEvents)
                .Select(m => m.SorFileId).ToArray();
            _logFile.AppendLine($"{ids.Length} measurements chosen for deletion");
            var count = await _sorFileRepository.RemoveManySorAsync(ids);
            _logFile.AppendLine($"{count} measurements deleted");
            _logFile.AppendLine("Optimization of SorFiles table started");

            var task = Task.Factory.StartNew(_eventStoreInitializer.OptimizeSorFilesTable);

            while (true)
            {
                _logFile.AppendLine("Check Optimization");
                Thread.Sleep(1000);
                var files = new DirectoryInfo(dir).GetFiles();
                var fileInfo = files.FirstOrDefault(f => f.Name.StartsWith("#sql"));
                if (fileInfo == null) break;
                _logFile.AppendLine($"{fileInfo.Name}   {fileInfo.Length}");

            }
            _logFile.AppendLine("SorFiles table is optimized");

            return count;
        }

        private async Task<string> MakeSnapshot(MakeSnapshot cmd)
        {
            await Task.Delay(1);
            return "";
        }

    }
}
