using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceForClient
    {
        private async Task<string> MakeSnapshot(MakeSnapshot cmd, string username, string clientIp)
        {
            _logFile.AppendLine("Start making snapshot on another thread to release WCF client");
            var unused = await Task.Factory.StartNew(() => FullProcedure(cmd, username, clientIp));
            _logFile.AppendLine("Snapshot started on another thread to release WCF client");
            return null;
        }

        private async Task FullProcedure(MakeSnapshot cmd, string username, string clientIp)
        {
            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto(){Stage = DbOptimizationStage.Starting});

            var result = await _eventStoreService.SendCommand(cmd, username, clientIp);
            _logFile.AppendLine(result);

            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto()
            {
                Stage = DbOptimizationStage.Done,
            });

            _logFile.AppendLine("Unblocking connections");
            await _d2CWcfManager.UnBlockClientAfterDbOptimization();
        }
    }
}
