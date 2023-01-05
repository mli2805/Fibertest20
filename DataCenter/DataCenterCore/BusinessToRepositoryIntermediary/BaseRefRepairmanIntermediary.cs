using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Iit.Fibertest.DatabaseLibrary;
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
        private readonly TraceModelBuilder _traceModelBuilder;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        private string _culture;

        public BaseRefRepairmanIntermediary(Model writeModel, IniFile iniFile, IMyLog logFile, EventStoreService eventStoreService,
            SorFileRepository sorFileRepository, 
            TraceModelBuilder traceModelBuilder, BaseRefLandmarksTool baseRefLandmarksTool,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter)
        {
            _writeModel = writeModel;
            _logFile = logFile;
            _eventStoreService = eventStoreService;
            _sorFileRepository = sorFileRepository;
            _traceModelBuilder = traceModelBuilder;
            _baseRefLandmarksTool = baseRefLandmarksTool;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;

            _culture = iniFile.Read(IniSection.General, IniKey.Culture, "en-US");
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
                var listOfBaseRef = await GetBaseRefDtos(trace);

                if (!listOfBaseRef.Any())
                    return string.Format(Resources.SID_Can_t_get_base_refs_for_trace__0_, trace.TraceId.First6());

                foreach (var baseRefDto in listOfBaseRef.Where(b => b.SorFileId > 0))
                {
                    Modify(trace, baseRefDto);
                    if (await _sorFileRepository.UpdateSorBytesAsync(baseRefDto.SorFileId, baseRefDto.SorBytes) == -1)
                        return Resources.SID_Can_t_save_amended_reflectogram;
                }

                if (trace.OtauPort == null) // unattached trace
                    continue;

                var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
                if (rtu == null)
                    return "RTU not found.";

                if (rtu.IsAvailable)
                {
                    var result = await SendAmendedBaseRefsToRtu(rtu, trace, listOfBaseRef);
                    if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                        returnStr = result.ErrorMessage;

                    var updateResult = await UpdateVeexTestList(result, "system", "localhost");
                    if (updateResult.ReturnCode != ReturnCode.Ok)
                        returnStr = updateResult.ErrorMessage;
                }
                else
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(_culture);
                    returnStr = Resources.SID_Failed_to_resend_base_to_rtu;
                }
            }

            return returnStr;
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

        private void Modify(Trace trace, BaseRefDto baseRefDto)
        {
            try
            {
                var traceModel = _writeModel.GetTraceComponentsByIds(trace);
                var modelWithDistances = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(traceModel);
                var sorData = SorData.FromBytes(baseRefDto.SorBytes);
                _baseRefLandmarksTool.SetLandmarksLocation(sorData, modelWithDistances);
                _baseRefLandmarksTool.AddNamesAndTypesForLandmarks(sorData, modelWithDistances);
                baseRefDto.SorBytes = sorData.ToBytes();
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Amend base ref - Modify: {e.Message}");
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
