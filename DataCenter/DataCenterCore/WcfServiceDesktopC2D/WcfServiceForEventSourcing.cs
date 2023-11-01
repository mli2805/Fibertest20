using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using NEventStore;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceDesktopC2D
    {
        public async Task<int> SendCommands(List<string> jsons, string username, string clientIp) // especially for Migrator.exe
        {
            var cmds = jsons.Select(json => JsonConvert.DeserializeObject(json, JsonSerializerSettings)).ToList();

            await _eventStoreService.SendCommands(cmds, username, clientIp);
            return jsons.Count;
        }

        public async Task<int> SendMeas(List<AddMeasurementFromOldBase> list)
        {
            foreach (var dto in list)
            {
                var sorId = await _sorFileRepository.AddSorBytesAsync(dto.SorBytes);
                if (sorId == -1) return -1;

                var command = _measurementFactory.CreateCommand(dto, sorId);
                await _eventStoreService.SendCommand(command, "migrator", "OnServer");
            }

            return 0;
        }

        public async Task<int> SendCommandsAsObjs(List<object> cmds)
        {
            // during the tests "client" invokes not the C2DWcfManager's method to communicate by network
            // but right server's method from WcfServiceForClient
            var username = "NCrunch";
            var clientIp = "127.0.0.1"; 
            var list = new List<string>();

            foreach (var cmd in cmds)
            {
                list.Add(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings));
            }

            return await SendCommands(list, username, clientIp);
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            // during the tests "client" invokes not the C2DWcfManager's method to communicate by network
            // but right server's method from WcfServiceForClient
            var username = "NCrunch";
            var clientIp = "127.0.0.1";
            return await SendCommand(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings), username, clientIp);
        }

        public async Task<string> SendCommand(string json, string username, string clientIp)
        {
            var cmd = JsonConvert.DeserializeObject(json, JsonSerializerSettings);

            if (cmd is RemoveEventsAndSors removeEventsAndSors)
            {
                await Task.Factory.StartNew(() => RemoveEventsAndSors(removeEventsAndSors, username, clientIp));
                return null;
            }

            if (cmd is MakeSnapshot makeSnapshot)
            {
                _logFile.AppendLine($"{username} from {clientIp} asked to make snapshot");
                await Task.Factory.StartNew(() => MakeSnapshot(makeSnapshot, username, clientIp));
                return null;
            }

            if (cmd is CleanTrace cleanTrace) // only removes sor files, Trace will be cleaned further
            {
                var res = await RemoveSorFiles(cleanTrace.TraceId);
                if (!string.IsNullOrEmpty(res)) return res;
            }

            if (cmd is RemoveTrace removeTrace) // only removes sor files, Trace will be removed further
            {
                var res = await RemoveSorFiles(removeTrace.TraceId);
                if (!string.IsNullOrEmpty(res)) return res;
            }

            var resultInGraph = await _eventStoreService.SendCommand(cmd, username, clientIp);
            if (resultInGraph != null)
                return resultInGraph;

            // Some commands need to be reported to web client
            await NotifyWebClient(cmd);

            // A few commands need post-processing in Db or RTU
            return await PostProcessing(cmd);
        }

        private async Task<string> RemoveSorFiles(Guid traceId)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return $@"Trace {traceId} not found";

            // starting in another thread breaks tests
            // await Task.Factory.StartNew(() => LongPart(traceId, cleanTrace, username, clientIp));

            return await LongPart(traceId);
        }

        private async Task<string> LongPart(Guid traceId)
        {
            var sorFileIds = _writeModel.Measurements
                .Where(m => m.TraceId == traceId)
                .Select(l => l.SorFileId).ToArray();
            var sorFileIds2 = sorFileIds.Concat(_writeModel.BaseRefs
                    .Where(b => b.TraceId == traceId)
                    .Select(l => l.SorFileId).ToArray())
                .ToArray();
            await _sorFileRepository.RemoveManySorAsync(sorFileIds2);
            return null;
        }

        private async Task<string> PostProcessing(object cmd)
        {
            if (cmd is RemoveRtu removeRtu)
                return await _rtuStationsRepository.RemoveRtuAsync(removeRtu.RtuId);

            #region Base ref amend
            if (cmd is UpdateAndMoveNode updateAndMoveNode)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(updateAndMoveNode.NodeId);
            if (cmd is UpdateRtu updateRtu)
                return await _baseRefRepairmanIntermediary.AmendForTracesFromRtu(updateRtu.RtuId);
            if (cmd is UpdateNode updateNode)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(updateNode.NodeId);
            if (cmd is MoveNode moveNode)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(moveNode.NodeId);
            if (cmd is UpdateEquipment updateEquipment)
                return await _baseRefRepairmanIntermediary.ProcessUpdateEquipment(updateEquipment.EquipmentId);
            if (cmd is UpdateFiber updateFiber)
                return await _baseRefRepairmanIntermediary.ProcessUpdateFiber(updateFiber.Id);
            if (cmd is AddNodeIntoFiber addNodeIntoFiber)
                return await _baseRefRepairmanIntermediary.AmendForTracesWhichUseThisNode(addNodeIntoFiber.Id);
            if (cmd is RemoveNode removeNode && removeNode.IsAdjustmentPoint)
                return await _baseRefRepairmanIntermediary.ProcessNodeRemoved(removeNode.DetoursForGraph.Select(d => d.TraceId)
                    .ToList());
            #endregion

            return null;
        }

        public async Task<string[]> GetEvents(GetEventsDto dto)
        {
            return await Task.FromResult(_eventStoreService.GetEvents(dto.Revision));
        }

        private byte[] _serializedModel;
        private const int PortionSize = 2 * 1024 * 1024;
        static readonly object LockObj = new object();
        public async Task<SerializedModelDto> GetModelDownloadParams(GetSnapshotDto dto)
        {
            await Task.Delay(1);
            _logFile.AppendLine("Model asked by client");
            lock (LockObj)
            {
                _serializedModel = _writeModel.Serialize(_logFile).Result;
            }
            _logFile.AppendLine("Model serialized successfully");

            return new SerializedModelDto()
            {
                PortionsCount = _serializedModel.Length / PortionSize + 1,
                Size = _serializedModel.Length,
                LastIncludedEvent = _eventStoreService.StoreEvents.OpenStream(_eventStoreService.StreamIdOriginal).StreamRevision,
            };
        }

        public async Task<byte[]> GetModelPortion(int portionOrdinal)
        {
            await Task.Delay(1);
            var currentPortionSize = PortionSize * (portionOrdinal + 1) < _serializedModel.Length
                ? PortionSize
                : _serializedModel.Length - PortionSize * (portionOrdinal);
            var portion = new byte[currentPortionSize];
            Array.Copy(_serializedModel, PortionSize * portionOrdinal,
                portion, 0, currentPortionSize);

            return portion;
        }

    }
}
