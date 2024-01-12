using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class BaseRefRepairmanIntermediary
    {
        private readonly Model _writeModel;
        private readonly IMyLog _logFile;
        private readonly EventStoreService _eventStoreService;
        private readonly SorFileRepository _sorFileRepository;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        public BaseRefRepairmanIntermediary(Model writeModel, IMyLog logFile, EventStoreService eventStoreService,
            SorFileRepository sorFileRepository, BaseRefLandmarksTool baseRefLandmarksTool,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter)
        {
            _writeModel = writeModel;
            _logFile = logFile;
            _eventStoreService = eventStoreService;
            _sorFileRepository = sorFileRepository;
            _baseRefLandmarksTool = baseRefLandmarksTool;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
        }

        private static readonly IMapper Mapper1 = new MapperConfiguration(
            cfg => cfg.AddProfile<VeexTestMappingProfile>()).CreateMapper();

        public async Task<RequestAnswer> UpdateVeexTestList(BaseRefAssignedDto transferResult, string username, string clientIp)
        {
            var commands = transferResult.AddVeexTests
                .Select(l => (object)Mapper1.Map<AddVeexTest>(l)).ToList();

            var ccc = transferResult.RemoveVeexTests.Select(l => new RemoveVeexTest() { TestId = l });
            commands.AddRange(ccc);

            var cmdCount = await _eventStoreService.SendCommands(commands, username, clientIp);

            return cmdCount == commands.Count
                ? new RequestAnswer() { ReturnCode = ReturnCode.Ok }
                : new RequestAnswer()
                {
                    ReturnCode = ReturnCode.Error,
                    ErrorMessage = "Failed to apply commands maintaining veex tests table!"
                };
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
            var tracesWhichUseThisEquipment = _writeModel.Traces
                .Where(t => t.EquipmentIds.Contains(equipmentId) && t.HasAnyBaseRef).ToList();
            return await AmendBaseRefs(tracesWhichUseThisEquipment);
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
            string returnStr = null;
            foreach (var trace in traces)
            {
                var res = await AmendBaseRefsForOneTrace(trace);
                if (res.ReturnCode != ReturnCode.Ok)
                    returnStr = res.ErrorMessage;
            }

            return returnStr;
        }

        public async Task<RequestAnswer> AmendBaseRefsForOneTrace(Trace trace)
        {
            var listOfBaseRef = await GetBaseRefDtos(trace);

            if (!listOfBaseRef.Any())
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.FailedToGetBaseRefs,
                    ErrorMessage = string.Format(Resources.SID_Can_t_get_base_refs_for_trace__0_,
                        trace.TraceId.First6())
                };

            foreach (var baseRefDto in listOfBaseRef.Where(b => b.SorFileId > 0))
            {
                var requestAnswer = Modify(trace, baseRefDto);
                if (requestAnswer.ReturnCode != ReturnCode.BaseRefsForTraceModifiedSuccessfully)
                    return requestAnswer;
                var saveResult = await _sorFileRepository
                    .UpdateSorBytesAsync(baseRefDto.SorFileId, baseRefDto.SorBytes);
                if (saveResult != null)
                    return new RequestAnswer()
                    {
                        ReturnCode = ReturnCode.FailedToSaveBaseRefs,
                        ErrorMessage = saveResult,
                    };
            }

            if (trace.OtauPort == null) // unattached trace
                return new RequestAnswer() { ReturnCode = ReturnCode.Ok };

            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null)
                return new RequestAnswer() { ReturnCode = ReturnCode.NoSuchRtu };

            if (rtu.IsAvailable)
            {
                var result = await SendAmendedBaseRefsToRtu(rtu, trace, listOfBaseRef);
                if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return new RequestAnswer()
                    {
                        ReturnCode = ReturnCode.FailedToSendBaseToRtu,
                        ErrorMessage = result.ErrorMessage,
                    };

                if (rtu.RtuMaker == RtuMaker.VeEX)
                {
                    var updateResult = await UpdateVeexTestList(result, "system", "localhost");
                    if (updateResult.ReturnCode != ReturnCode.Ok)
                        return new RequestAnswer()
                        {
                            ReturnCode = ReturnCode.FailedToUpdateVeexTestList,
                            ErrorMessage = updateResult.ErrorMessage,
                        };
                }
            }
            else
            {
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.FailedToSendBaseToRtu,
                };
            }

            return new RequestAnswer() { ReturnCode = ReturnCode.Ok };
        }

        private async Task<List<BaseRefDto>> GetBaseRefDtos(Trace trace)
        {
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
                    continue;
                listOfBaseRef.Add(baseRef.CreateFromBaseRef(sorBytes));
            }

            _logFile.AppendLine($"{listOfBaseRef.Count} base refs changed");
            return listOfBaseRef;
        }

        private RequestAnswer Modify(Trace trace, BaseRefDto baseRefDto)
        {
            try
            {
                var sorData = SorData.FromBytes(baseRefDto.SorBytes);

                _baseRefLandmarksTool.ApplyTraceToBaseRef(sorData, trace, false);

                baseRefDto.SorBytes = sorData.ToBytes();

                return new RequestAnswer() { ReturnCode = ReturnCode.BaseRefsForTraceModifiedSuccessfully };
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Amend base ref - Modify: {e.Message}");
                return new RequestAnswer() { ReturnCode = ReturnCode.FailedToModifyBaseRef, ErrorMessage = e.Message };
            }
        }

        private async Task<BaseRefAssignedDto> SendAmendedBaseRefsToRtu(Rtu rtu, Trace trace, List<BaseRefDto> baseRefDtos)
        {
            var dto = new AssignBaseRefsDto()
            {
                TraceId = trace.TraceId,
                RtuId = rtu.Id,
                RtuMaker = rtu.RtuMaker,
                OtdrId = rtu.OtdrId,
                OtauPortDto = trace.OtauPort,
                MainOtauPortDto = new OtauPortDto()
                {
                    OtauId = rtu.MainVeexOtau.id,
                    Serial = rtu.MainVeexOtau.serialNumber,
                    OpticalPort = trace.OtauPort.MainCharonPort,
                    IsPortOnMainCharon = true,
                },
                BaseRefs = baseRefDtos,
            };

            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.TransmitBaseRefsToRtuAsync(dto)
                : await _clientToRtuVeexTransmitter.TransmitBaseRefsToRtuAsync(dto);
        }
    }
}
