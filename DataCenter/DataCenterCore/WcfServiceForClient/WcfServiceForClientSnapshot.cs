﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using NEventStore;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceForClient
    {
        private async Task<string> MakeSnapshot(MakeSnapshot cmd, string username, string clientIp)
        {
            _logFile.AppendLine("Start making snapshot on another thread to release WCF client");
            var unused = await Task.Factory.StartNew(() => FullProcedure(cmd, username, clientIp));
            return null;
        }

        private async Task FullProcedure(MakeSnapshot cmd, string username, string clientIp)
        {
            var addresses = _clientsCollection.GetClientsAddresses();
            if (addresses == null)
                return;
            _d2CWcfManager.SetClientsAddresses(addresses);
            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto() { Stage = DbOptimizationStage.Starting });

            var tuple = await CreateModelUptoDate2(cmd.UpTo);
            var data = await tuple.Item2.Serialize(_logFile);
            var portionCount = await _snapshotRepository.AddSnapshotAsync(_eventStoreService.AggregateId, tuple.Item1, cmd.UpTo.Date, data);
            if (portionCount == -1) return;
            _eventStoreService.LastEventNumberInSnapshot = tuple.Item1;
            _eventStoreService.LastEventDateInSnapshot = cmd.UpTo.Date;
            var removedSnapshotPortions = await _snapshotRepository.RemoveOldSnapshots();
            _logFile.AppendLine($"{removedSnapshotPortions} portions of old snapshot removed");

            var result = await _eventStoreService.SendCommand(cmd, username, clientIp);
            if (result != null)
                _logFile.AppendLine(result);

            await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto()
            {
                Stage = DbOptimizationStage.SnapshotDone,
            });

            _logFile.AppendLine("Unblocking connections");
            await _d2CWcfManager.UnBlockClientAfterDbOptimization();
        }

        private async Task<Tuple<int, Model>> CreateModelUptoDate2(DateTime date)
        {
            var modelForSnapshot = new Model();

            // TODO block RTU events appearance

            var snapshot = await _snapshotRepository.ReadSnapshotAsync(_eventStoreService.AggregateId);
            var lastIncludedEvent = snapshot.Item1;
            if (lastIncludedEvent != 0)
            {
                var unused = await modelForSnapshot.Deserialize(_logFile, snapshot.Item2);
            }
            var eventStream = _eventStoreService.StoreEvents.OpenStream(_eventStoreService.AggregateId);
            var events = eventStream.CommittedEvents
                .Where(x => ((DateTime)x.Headers["Timestamp"]).Date <= date.Date)
                .Skip(lastIncludedEvent).ToList();
            _logFile.AppendLine($"{events.Count} events should be applied...");
            foreach (var evnt in events)
            {
                modelForSnapshot.Apply(evnt.Body);
                lastIncludedEvent++;
                if (lastIncludedEvent % 1000 == 0)
                {
                    _logFile.AppendLine($"{lastIncludedEvent}");
                    await _d2CWcfManager.BlockClientWhileDbOptimization(new DbOptimizationProgressDto()
                    {
                        Stage = DbOptimizationStage.ModelCreating,
                        EventsReplayed = lastIncludedEvent,
                    });
                }
            }

            _logFile.AppendLine($"last included event {lastIncludedEvent}");
            var result = new Tuple<int, Model>(lastIncludedEvent, modelForSnapshot);
            return result;
        }
    }
}
