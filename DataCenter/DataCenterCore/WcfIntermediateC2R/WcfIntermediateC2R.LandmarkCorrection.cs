using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    public class LongOperationsData
    {
        /// <summary>
        /// Guid - ID of long operation, created by client when request sent
        ///
        /// Queue - all progress lines of the same long operation
        /// string - json of dto which should be returned when progress asked
        ///
        /// i.e. long operation is LandmarksCorrection, so in json should be CorrectionProgressDto
        /// </summary>

        public readonly Dictionary<Guid, ConcurrentQueue<string>> Data = new Dictionary<Guid, ConcurrentQueue<string>>();
    }

    public partial class WcfIntermediateC2R
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        public async Task<CorrectionProgressDto> GetLandmarksCorrectionProgress(Guid batchId)
        {
            await Task.Delay(0);
            if (!_longOperationsData.Data.ContainsKey(batchId)) return null;

            var result = _longOperationsData.Data[batchId].TryDequeue(out string value);
            if (!result) return null;

            var resultDto = JsonConvert.DeserializeObject<CorrectionProgressDto>(value);
            return resultDto;
        }

        public async Task<CorrectionProgressDto> StartLandmarksCorrection(LandmarksCorrectionDto changesList)
        {
            // Event Sourcing
            await _eventStoreService.SendCommands(changesList.Corrections
                    .Select(j => JsonConvert.DeserializeObject(j, JsonSerializerSettings)).ToList(),
                "", changesList.ClientIp);


            // Re-send base refs
            var traces = GetTracesInvolved(changesList);
#pragma warning disable CS4014
            Task.Factory.StartNew(() => UpdateBaseRefs(changesList.BatchId, traces));
#pragma warning restore CS4014

            var result = new CorrectionProgressDto()
            {
                BatchId = changesList.BatchId,
                ReturnCode = ReturnCode.LandmarkChangesAppliedSuccessfully,
                TracesCorrected = 0,
                AllTracesInvolved = traces.Count,
            };

            return result;
        }

        private List<Trace> GetTracesInvolved(LandmarksCorrectionDto changesList)
        {
            var result = new Dictionary<Guid, Trace>();
            foreach (var trace in changesList.Corrections
                         .Select(GetTracesThrough)
                         .SelectMany(traces => traces.Where(trace => !result.ContainsKey(trace.TraceId))))
                result.Add(trace.TraceId, trace);

            return result.Values.ToList();
        }

       
        private IEnumerable<Trace> GetTracesThrough(string json)
        {
            var obj = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            switch (obj)
            {
                case UpdateAndMoveNode cmd:
                    return _writeModel.Traces.Where(t => t.NodeIds.Contains(cmd.NodeId));
                case UpdateFiber cmd:
                    return _writeModel.Traces.Where(t => t.FiberIds.Contains(cmd.Id));
                case UpdateEquipment cmd:
                    return _writeModel.Traces.Where(t => t.EquipmentIds.Contains(cmd.EquipmentId));
                case IncludeEquipmentIntoTrace cmd:
                    return _writeModel.Traces.Where(t => t.TraceId == cmd.TraceId);
            }
            return null;
        }

        private async Task UpdateBaseRefs(Guid longOperationId, List<Trace> allTracesInvolved)
        {
            var cc = 0;
            foreach (var trace in allTracesInvolved)
            {
                var res = await _baseRefRepairmanIntermediary.AmendBaseRefsForOneTrace(trace);

                var entry = new CorrectionProgressDto()
                {
                    BatchId = longOperationId,
                    TracesCorrected = ++cc,
                    TraceId = trace.TraceId,
                    AllTracesInvolved = allTracesInvolved.Count,
                    ReturnCode = res.ReturnCode,
                    ErrorMessage = res.ErrorMessage,
                };

                _logFile.AppendLine($"{cc} trace updated");
                if (!_longOperationsData.Data.ContainsKey(longOperationId)) 
                    _longOperationsData.Data.Add(longOperationId, new ConcurrentQueue<string>());
                _longOperationsData.Data[longOperationId]
                    .Enqueue( JsonConvert.SerializeObject(entry, JsonSerializerSettings));
                _logFile.AppendLine($"Queue contains {_longOperationsData.Data[longOperationId].Count} entries");
            }
        }
    }
}
