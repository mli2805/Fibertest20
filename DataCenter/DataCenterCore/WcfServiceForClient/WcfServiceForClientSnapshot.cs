using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Newtonsoft.Json;
using NEventStore;

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
            var addresses = _clientsCollection.GetClientsAddresses();
            if (addresses == null)
                return;
            _d2CWcfManager.SetClientsAddresses(addresses);  
            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto() { Stage = DbOptimizationStage.Starting });

            var tuple = await CreateModelUptoDate(cmd.UpTo);

            var data = await tuple.Item2.Serialize(_logFile);
            var addSnapshot = await _snapshotRepository.AddSnapshotAsync(_eventStoreService.GraphDbVersionId, tuple.Item1, data);
            if (addSnapshot == -1) return;
            var remove = await _snapshotRepository.RemoveOldSnapshots(_eventStoreService.GraphDbVersionId);

            // test
//            var re = await _snapshotRepository.ReadSnapshotAsync(_eventStoreService.GraphDbVersionId);
//            var model = await ModelSerializationExt.Deserialize(_logFile, re.Item2);



            var result = await _eventStoreService.SendCommand(cmd, username, clientIp);
            _logFile.AppendLine(result);

            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto()
            {
                Stage = DbOptimizationStage.Done,
            });

            _logFile.AppendLine("Unblocking connections");
            await _d2CWcfManager.UnBlockClientAfterDbOptimization();
        }

        private async Task<Tuple<int, Model>> CreateModelUptoDate(DateTime date)
        {
            var modelForSnapshot = new Model();

            var currentEventNumber = 0;

            while (true)
            {
                var isUpto = false;
                var events = _eventStoreService.GetEvents(currentEventNumber);
                if (events.Length == 0)
                    break;
                foreach (var json in events)
                {
                    var msg = (EventMessage)JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                    if (((DateTime)msg.Headers["Timestamp"]).Date > date.Date)
                    {
                        isUpto = true;
                        break;
                    }
                    modelForSnapshot.Apply(msg.Body);
                    currentEventNumber++;
                }
                if (isUpto) break;
                _logFile.AppendLine($"{currentEventNumber}");
                await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto()
                {
                    Stage = DbOptimizationStage.ModelCreating,
                    Recreated = currentEventNumber,
                });
            }

            _logFile.AppendLine($"last included event {currentEventNumber}");
            var result = new Tuple<int, Model>(currentEventNumber, modelForSnapshot);
            return result;
        }
    }
}
