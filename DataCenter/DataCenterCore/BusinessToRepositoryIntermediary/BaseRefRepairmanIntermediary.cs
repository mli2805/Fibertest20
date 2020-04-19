using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class BaseRefRepairmanIntermediary
    {
        private readonly Model _writeModel;
        private readonly IMyLog _logFile;
        private readonly SorFileRepository _sorFileRepository;
        private readonly BaseRefDtoFactory _baseRefDtoFactory;
        private readonly TraceModelBuilder _traceModelBuilder;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;

        public BaseRefRepairmanIntermediary(Model writeModel, IMyLog logFile,
            SorFileRepository sorFileRepository, BaseRefDtoFactory baseRefDtoFactory,
            TraceModelBuilder traceModelBuilder, BaseRefLandmarksTool baseRefLandmarksTool,
            ClientToRtuTransmitter clientToRtuTransmitter)
        {
            _writeModel = writeModel;
            _logFile = logFile;
            _sorFileRepository = sorFileRepository;
            _baseRefDtoFactory = baseRefDtoFactory;
            _traceModelBuilder = traceModelBuilder;
            _baseRefLandmarksTool = baseRefLandmarksTool;
            _clientToRtuTransmitter = clientToRtuTransmitter;
        }

        public async Task<string> AmendForTracesWhichUseThisNode(Guid nodeId)
        {
            var tracesWhichUseThisNode = _writeModel.Traces.Where(t => t.NodeIds.Contains(nodeId) && t.HasAnyBaseRef).ToList();
            return await AmendBaseRefs(tracesWhichUseThisNode);
        }

        public async Task<string> AmendForTracesFromRtu(Guid rtuId)
        {
            var traceFromRtu = _writeModel.Traces.Where(t => t.RtuId == rtuId && t.HasAnyBaseRef).ToList();
            return await AmendBaseRefs(traceFromRtu);
        }

        public async Task<string> ProcessUpdateEquipment(Guid equipmentId)
        {
            var equipment = _writeModel.Equipments.FirstOrDefault(e => e.EquipmentId == equipmentId);
            if (equipment == null)
                return $"Can't find equipment {equipmentId}";

            var node = _writeModel.Nodes.FirstOrDefault(n => n.NodeId == equipment.NodeId);
            if (node == null)
                return $"Can't find node {equipment.NodeId}";

            var tracesWhichUseThisNode = _writeModel.Traces.Where(t => t.NodeIds.Contains(node.NodeId) && t.HasAnyBaseRef).ToList();
            return await AmendBaseRefs(tracesWhichUseThisNode);
        }

        public async Task<string> ProcessUpdateFiber(Guid fiberId)
        {
            var tracesWhichUseThisFiber = _writeModel.GetTracesPassingFiber(fiberId).Where(t => t.HasAnyBaseRef).ToList();
            return await AmendBaseRefs(tracesWhichUseThisFiber);
        }

        public async Task<string> ProcessNodeRemoved(List<Guid> traceIds)
        {
            var tracesWhichUsedThisNode = new List<Trace>();
            foreach (var id in traceIds)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == id);
                if (trace != null && trace.HasAnyBaseRef)
                    tracesWhichUsedThisNode.Add(trace);
            }
            return await AmendBaseRefs(tracesWhichUsedThisNode);
        }

        private async Task<string> AmendBaseRefs(List<Trace> traces)
        {
            foreach (var trace in traces)
            {
                var listOfBaseRef = await GetBaseRefDtos(trace);

                if (!listOfBaseRef.Any())
                    return string.Format(Resources.SID_Can_t_get_base_refs_for_trace__0_, trace.TraceId.First6());

                foreach (var baseRefDto in listOfBaseRef.Where(b=>b.SorFileId > 0))
                {
                    Modify(trace, baseRefDto);
                    if (await _sorFileRepository.UpdateSorBytesAsync(baseRefDto.SorFileId, baseRefDto.SorBytes) == -1)
                        return Resources.SID_Can_t_save_amended_reflectogram;
                }

                if (trace.OtauPort == null) // unattached trace
                    continue;

                var result = await SendAmendedBaseRefsToRtu(trace, listOfBaseRef);
                if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return result.ErrorMessage;
            }

            return null;
        }

        private async Task<List<BaseRefDto>> GetBaseRefDtos(Trace trace)
        {
          // if (_writeModel.BaseRefs.Any(b=>b.Id == Guid.Empty))
          //      _logFile.AppendLine("There is a base ref with Empty ID!!!");
          //  else _logFile.AppendLine("All base refs have valid IDs");

            var list = new List<BaseRef>
            {
                _writeModel.BaseRefs.FirstOrDefault(b => b.Id == trace.PreciseId),
                _writeModel.BaseRefs.FirstOrDefault(b => b.Id == trace.FastId),
                _writeModel.BaseRefs.FirstOrDefault(b => b.Id == trace.AdditionalId && b.BaseRefType == BaseRefType.Additional)
            };

            var listOfBaseRef = new List<BaseRefDto>();

            foreach (var baseRef in list)
            {
                if (baseRef == null) continue;
                var sorBytes = await _sorFileRepository.GetSorBytesAsync(baseRef.SorFileId);
                if (sorBytes == null)
                {
            //        _logFile.AppendLine($"sorBytes not found for ID = {baseRef.SorFileId}, skip it!");
                    continue;
                }
                listOfBaseRef.Add(_baseRefDtoFactory.CreateFromBaseRef(baseRef, sorBytes));
            }

            _logFile.AppendLine($"{listOfBaseRef.Count} base refs changed");
            return listOfBaseRef;
        }

        private void Modify(Trace trace, BaseRefDto baseRefDto)
        {
            var traceModel = _writeModel.GetTraceComponentsByIds(trace);
            var modelWithDistances = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(traceModel);
            var sorData = SorData.FromBytes(baseRefDto.SorBytes);
            _baseRefLandmarksTool.SetLandmarksLocation(sorData, modelWithDistances);
            _baseRefLandmarksTool.AddNamesAndTypesForLandmarks(sorData, modelWithDistances);
            baseRefDto.SorBytes = sorData.ToBytes();
        }

        private async Task<BaseRefAssignedDto> SendAmendedBaseRefsToRtu(Trace trace, List<BaseRefDto> baseRefDtos)
        {
            var dto = new AssignBaseRefsDto()
            {
                TraceId = trace.TraceId,
                RtuId = trace.RtuId,
                OtauPortDto = trace.OtauPort,
                BaseRefs = baseRefDtos,
            };
            return await _clientToRtuTransmitter.TransmitBaseRefsToRtu(dto);
        }
    }
}
