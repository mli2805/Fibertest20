using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class RtuInitializationToGraphApplier
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly EventStoreService _eventStoreService;
        private readonly ClientsCollection _clientsCollection;

        public RtuInitializationToGraphApplier(IMyLog logFile, Model writeModel, EventStoreService eventStoreService,
            ClientsCollection clientsCollection
            )
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _eventStoreService = eventStoreService;
            _clientsCollection = clientsCollection;
        }

        public async Task<RtuInitializedDto> ApplyRtuInitializationResult(InitializeRtuDto dto, RtuInitializedDto result)
        {
            if (result.IsInitialized)
            {
                var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
                await _eventStoreService.SendCommands(DtoToCommandList(dto, result), username, dto.ClientIp);
            }
            return result;
        }

        private List<object> DtoToCommandList(InitializeRtuDto dto, RtuInitializedDto result)
        {
            var commandList = new List<object>();
            var originalRtu = _writeModel.Rtus.First(r => r.Id == result.RtuId);

            // Own port count changed
            if (originalRtu.OwnPortCount > result.OwnPortCount)
            {
                var traces = _writeModel.Traces.Where(t =>
                    t.RtuId == result.RtuId && t.Port >= result.OwnPortCount && t.OtauPort.Serial == originalRtu.Serial);
                foreach (var trace in traces)
                {
                    var cmd = new DetachTrace() { TraceId = trace.TraceId };
                    commandList.Add(cmd);
                }
            }

            // main veex otau state changed
            if (!dto.IsFirstInitialization && 
                originalRtu.MainVeexOtau.connected != result.MainVeexOtau.connected)
            {
                commandList.Add(new AddBopNetworkEvent()
                {
                    EventTimestamp = DateTime.Now,
                    RtuId = result.RtuId,
                    Serial = originalRtu.Serial,
                    OtauIp = originalRtu.OtdrNetAddress.Ip4Address,
                    TcpPort = originalRtu.OtdrNetAddress.Port,
                    IsOk = result.MainVeexOtau.connected,
                });
            }

            // BOP state changed
            if (result.Children != null)
                foreach (var keyValuePair in result.Children)
                {
                    var bop = _writeModel.Otaus.FirstOrDefault(o => o.NetAddress.Equals(keyValuePair.Value.NetAddress));
                    if (bop == null)
                    {
                        // This happens when Khazanov writes into RTU's ini file while RTU works
                        // should not happen in real life but anyway
                        result.Children.Remove(keyValuePair.Key);
                        _logFile.AppendLine($"There is no bop with address {keyValuePair.Value.NetAddress.ToStringA()} in graph");
                        continue;
                    }
                    if (bop.IsOk != keyValuePair.Value.IsOk)
                        commandList.Add(new AddBopNetworkEvent()
                        {
                            EventTimestamp = DateTime.Now,
                            RtuId = result.RtuId,
                            Serial = keyValuePair.Value.Serial ?? bop.Serial,
                            OtauIp = keyValuePair.Value.NetAddress.Ip4Address,
                            TcpPort = keyValuePair.Value.NetAddress.Port,
                            IsOk = keyValuePair.Value.IsOk,
                        });
                }

            commandList.Add(GetInitializeRtuCommand(dto, result));
            return commandList;
        }

        private InitializeRtu GetInitializeRtuCommand(InitializeRtuDto dto, RtuInitializedDto result)
        {
            var cmd = new InitializeRtu
            {
                Id = result.RtuId,
                Maker = result.Maker,
                OtdrId = result.OtdrId,
                MainVeexOtau = result.MainVeexOtau,
                Mfid = result.Mfid,
                Mfsn = result.Mfsn,
                Omid = result.Omid,
                Omsn = result.Omsn,
                MainChannel = dto.RtuAddresses.Main,
                MainChannelState = RtuPartState.Ok,
                IsReserveChannelSet = dto.RtuAddresses.HasReserveAddress,
                ReserveChannel = dto.RtuAddresses.HasReserveAddress
                    ? dto.RtuAddresses.Reserve
                    : null,
                ReserveChannelState = dto.RtuAddresses.HasReserveAddress ? RtuPartState.Ok : RtuPartState.NotSetYet,
                OtauNetAddress = result.OtdrAddress,
                OwnPortCount = result.OwnPortCount,
                FullPortCount = result.FullPortCount,
                Serial = result.Serial,
                Version = result.Version,
                Version2 = result.Version2,
                IsMonitoringOn = result.IsMonitoringOn,
                Children = result.Children,
                AcceptableMeasParams = result.AcceptableMeasParams,
            };
            return cmd;
        }
    }
}
