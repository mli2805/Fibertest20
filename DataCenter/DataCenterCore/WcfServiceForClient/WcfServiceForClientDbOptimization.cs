using System.Linq;
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
            if (!cmd.IsMeasurementsNotEvents && !cmd.IsOpticalEvents) return 0;

            _logFile.AppendLine("Start SorFiles cleaning");
            var ids = _writeModel.GetMeasurementsForDeletion(cmd.UpTo, cmd.IsMeasurementsNotEvents, cmd.IsOpticalEvents)
                .Select(m => m.SorFileId).ToArray();
            _logFile.AppendLine($"{ids.Length} measurements chosen for deletion");
            var count = await _sorFileRepository.RemoveManySorAsync(ids);
            _logFile.AppendLine($"{count} measurements deleted");
            var code = _eventStoreInitializer.OptimizeSorFilesTable();
            _logFile.AppendLine($"SorFiles table is optimized, return code is {code}");

            return count;
        }

        private async Task<string> MakeSnapshot(MakeSnapshot cmd)
        {
            await Task.Delay(1);
            return "";
        }

    }
}
