using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceForClient
    {
        private async Task<string> RemoveEventsAndSors(RemoveEventsAndSors removeEventsAndSors, string username, string clientIp)
        {
            _logFile.AppendLine("Start DB optimization on another thread to release WCF client");
            var unused = await Task.Factory.StartNew(() => FullProcedure(removeEventsAndSors, username, clientIp));
            _logFile.AppendLine("DB optimization started on another thread to release WCF client");
            return null;
        }

        private async Task FullProcedure(RemoveEventsAndSors removeEventsAndSors, string username, string clientIp)
        {
            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto(){Stage = DbOptimizationStage.Starting});
            // block RTUs messages (MSMQ and notifications)
            // block new client's attempts to register
            _globalState.IsDatacenterInDbOptimizationMode = true;

            _logFile.AppendLine("block is ON");
            var oldSize = _eventStoreInitializer.GetDataSize() / 1e9;
            var unused = await ClearSor(removeEventsAndSors);

            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto(){Stage = DbOptimizationStage.ModelAdjusting});
            _logFile.AppendLine("Removing from writeModel");
            await _eventStoreService.SendCommand(removeEventsAndSors, username, clientIp);

            _logFile.AppendLine("Unblocking connections");
            _globalState.IsDatacenterInDbOptimizationMode = false;
            var newSize = _eventStoreInitializer.GetDataSize() / 1e9;
            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto()
            {
                Stage = DbOptimizationStage.Done,
                OldSizeGb = oldSize, NewSizeGb = newSize,
            });
            await _d2CWcfManager.UnBlockClientAfterDbOptimization();
        }

        private async Task<int> ClearSor(RemoveEventsAndSors cmd)
        {
            _logFile.AppendLine("Start SorFiles cleaning");
            if (!cmd.IsMeasurementsNotEvents && !cmd.IsOpticalEvents) return 0;

            var ids = _writeModel.GetMeasurementsForDeletion(cmd.UpTo, cmd.IsMeasurementsNotEvents, cmd.IsOpticalEvents)
                .Select(m => m.SorFileId).ToArray();
            _logFile.AppendLine($"{ids.Length} measurements chosen for deletion");
            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto()
            {
                Stage = DbOptimizationStage.SorsRemoving, MeasurementsChosenForDeletion = ids.Length,
            });
            var count = await _sorFileRepository.RemoveManySorAsync(ids);
            _logFile.AppendLine($"{count} measurements removed");

            var dir = _eventStoreInitializer.DataDir;
            long oldSize = SorFileSize(dir);
            _logFile.AppendLine($"Optimization of sorfiles.ibd {oldSize} started");


            var unused = Task.Factory.StartNew(_eventStoreInitializer.OptimizeSorFilesTable);
            _logFile.AppendLine("Optimization process started on another thread");

            _logFile.AppendLine("And we will check sorfiles.ibd and #sql-ib.....ibd size to know if optimization is finished");
            while (true)
            {
                Thread.Sleep(2000);

                var files = new DirectoryInfo(dir + @"ft20efcore\").GetFiles();
                var fileInfo = files.FirstOrDefault(f => f.Name.StartsWith("#sql-ib"));
                if (fileInfo == null) break;
                var proc = fileInfo.Length * 100.0 / oldSize;
                _logFile.AppendLine($"{fileInfo.Name}   {proc}%  copied");
                await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto()
                {
                    Stage = DbOptimizationStage.TableCompressing, Copied = proc,
                });

            }
            var newSize = SorFileSize(dir);
            _logFile.AppendLine($"SorFiles table is optimized, new size is {newSize}, profit is {oldSize - newSize} bytes");

            return count;
        }

        private long SorFileSize(string dir)
        {
            try
            {
                var sorFileInfo = new FileInfo(dir + @"ft20efcore\sorfiles.ibd");
                return sorFileInfo.Length;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }

            return -1;
        }

    }
}
