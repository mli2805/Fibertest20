using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;

namespace Iit.Fibertest.DataCenterCore
{
    public class BaseRefRepairmanIntermediary
    {
        private readonly WriteModel _writeModel;
        private readonly BaseRefRepairman _baseRefRepairman;
        private readonly BaseRefsRepositoryIntermediary _baseRefsRepositoryIntermediary;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;

        public BaseRefRepairmanIntermediary(WriteModel writeModel, BaseRefRepairman baseRefRepairman,
            BaseRefsRepositoryIntermediary baseRefsRepositoryIntermediary, ClientToRtuTransmitter clientToRtuTransmitter)
        {
            _writeModel = writeModel;
            _baseRefRepairman = baseRefRepairman;
            _baseRefsRepositoryIntermediary = baseRefsRepositoryIntermediary;
            _clientToRtuTransmitter = clientToRtuTransmitter;
        }

        public async Task<string> ProcessNodeMoved(Guid nodeId)
        {
            var tracesWhichUseThisNode = _writeModel.Traces.Where(t => t.Nodes.Contains(nodeId)).ToList();
            return await AmendBaseRefs(tracesWhichUseThisNode);
        }

        public async Task<string> ProcessUpdateEquipment(Guid equipmentId)
        {
            var equipment = _writeModel.Equipments.FirstOrDefault(e => e.Id == equipmentId);
            if (equipment == null)
                return $"Can't find equipment {equipmentId}";

            var node = _writeModel.Nodes.FirstOrDefault(n => n.Id == equipment.NodeId);
            if (node == null)
                return $"Can't find node {equipment.NodeId}";

            var tracesWhichUseThisNode = _writeModel.Traces.Where(t => t.Nodes.Contains(node.Id)).ToList();
            return await AmendBaseRefs(tracesWhichUseThisNode);
        }

        public async Task<string> ProcessUpdateFiber(Guid fiberId)
        {
            var tracesWhichUseThisFiber = _writeModel.GetTracesPassingFiber(fiberId).ToList();
            return await AmendBaseRefs(tracesWhichUseThisFiber);
        }

        public async Task<string> ProcessAddIntoFiber(Guid nodeId)
        {
            var tracesWhichUseThisNode = _writeModel.Traces.Where(t => t.Nodes.Contains(nodeId)).ToList();
            return await AmendBaseRefs(tracesWhichUseThisNode);
        }

        public async Task<string> ProcessNodeRemoved(List<Guid> traceIds)
        {
            var tracesWhichUsedThisNode = new List<Trace>();
            foreach (var id in traceIds)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.Id == id);
                if (trace != null)
                    tracesWhichUsedThisNode.Add(trace);
            }
            return await AmendBaseRefs(tracesWhichUsedThisNode);
        }

        private async Task<string> AmendBaseRefs(List<Trace> traces)
        {
            foreach (var trace in traces)
            {
                var listOfBaseRef = await _baseRefsRepositoryIntermediary.GetTraceBaseRefsAsync(trace.Id);
                if (listOfBaseRef == null)
                    return $"Can't get base refs for trace {trace.Id}";

                foreach (var baseRefDto in listOfBaseRef)
                {
                    baseRefDto.SorBytes = _baseRefRepairman.Modify(trace, baseRefDto.SorBytes);
                }

                var dto = new AssignBaseRefsDto()
                {
                    TraceId = trace.Id,
                    RtuId = trace.RtuId,
                    OtauPortDto = trace.OtauPort,
                    BaseRefs = listOfBaseRef,
                };

                var saveResult = await _baseRefsRepositoryIntermediary.AssignBaseRefAsync(dto);
                if (saveResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return "Can't save amended reflectogram";

                if (dto.OtauPortDto == null) // unattached trace
                    return null;

                await _clientToRtuTransmitter.AssignBaseRefAsync(dto);
            }

            return null;
        }
    }
}
