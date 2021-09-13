using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceWebC2D 
    {
   
        public async Task<RtuInformationDto> GetRtuInformation(string username, Guid rtuId)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetRtuInformation");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var result = new RtuInformationDto();
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return result;
            result.RtuTitle = rtu.Title;
            var rtuNode = _writeModel.Nodes.FirstOrDefault(n => n.NodeId == rtu.NodeId);
            result.GpsCoors = rtuNode?.Position.ToDetailedString(GpsInputMode.DegreesMinutesAndSeconds) ?? "";
            result.Comment = rtu.Comment;
            return result;
        }

        public async Task<RtuNetworkSettingsDto> GetRtuNetworkSettings(string username, Guid rtuId)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetRtuNetworkSettings");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var result = new RtuNetworkSettingsDto();
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return result;
            result.RtuTitle = rtu.Title;
            result.Mfid = rtu.Mfid;
            result.Serial = rtu.Serial;
            result.Version = rtu.Version;
            result.Version2 = rtu.Version2;
            result.OwnPortCount = rtu.OwnPortCount;
            result.FullPortCount = rtu.FullPortCount;
            result.MainChannel = rtu.MainChannel.ToStringA();
            result.IsReserveChannelSet = rtu.IsReserveChannelSet;
            result.ReserveChannel = rtu.ReserveChannel.ToStringA();
            result.OtdrAddress = rtu.OtdrNetAddress.Ip4Address == "192.168.88.101" 
                ? $"{rtu.MainChannel.Ip4Address}:{rtu.OtdrNetAddress.Port}" 
                : rtu.OtdrNetAddress.ToStringA();
            return result;
        }

        public async Task<RtuStateDto> GetRtuState(string username, Guid rtuId)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetRtuState");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var result = new RtuStateDto();
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return result;
            result.RtuId = rtu.Id.ToString();
            result.RtuTitle = rtu.Title;
            result.MainChannel = rtu.MainChannel.ToStringA();
            result.MainChannelState = rtu.MainChannelState;
            result.IsReserveChannelSet = rtu.IsReserveChannelSet;
            result.ReserveChannel = rtu.ReserveChannel.ToStringA();
            result.ReserveChannelState = rtu.ReserveChannelState;

            result.BopState = rtu.BopState;
            result.MonitoringMode = rtu.MonitoringState;

            result.OwnPortCount = rtu.OwnPortCount;
            result.FullPortCount = rtu.FullPortCount;
            result.BopCount = rtu.Children.Count;

            result.Children = PrepareRtuStateChildren(rtu);
            result.TraceCount = result.Children.Count(c => c.TraceState != FiberState.NotInTrace && c.TraceState != FiberState.Nothing);
            result.TracesState = result.Children.Max(c => c.TraceState);

            return result;
        }

        private List<RtuStateChildDto> PrepareRtuStateChildren(Rtu rtu)
        {
            var result = new List<RtuStateChildDto>();
            for (int i = 1; i <= rtu.OwnPortCount; i++)
            {
                if (rtu.Children.ContainsKey(i))
                {
                    var otau = rtu.Children[i];
                    for (int j = 1; j <= otau.OwnPortCount; j++)
                    {
                        var trace = _writeModel.Traces.FirstOrDefault(t =>
                            t.OtauPort != null && t.OtauPort.Serial == otau.Serial && t.OtauPort.OpticalPort == j);
                        result.Add(trace != null
                                   ? PrepareRtuStateChild(trace, j, $"{i}-")
                                   : new RtuStateChildDto() { Port = $"{i}-{j}", TraceState = FiberState.Nothing });
                    }
                }
                else
                {
                    var trace = _writeModel.Traces.FirstOrDefault(t =>
                        t.RtuId == rtu.Id && t.Port == i && (t.OtauPort == null || t.OtauPort.IsPortOnMainCharon));
                    result.Add(trace != null
                        ? PrepareRtuStateChild(trace, i, "")
                        : new RtuStateChildDto() { Port = i.ToString(), TraceState = FiberState.Nothing });
                }
            }
            return result;
        }

        private RtuStateChildDto PrepareRtuStateChild(Trace trace, int port, string mainPort)
        {
            var prepareRtuStateChild = new RtuStateChildDto()
            {
                Port = mainPort + port,
                TraceId = trace.TraceId.ToString(),
                TraceTitle = trace.Title,
                TraceState = trace.State,
                LastMeasId = _writeModel.Measurements.LastOrDefault(m => m.TraceId == trace.TraceId)?.SorFileId.ToString() ?? "",
                LastMeasTime = _writeModel.Measurements.LastOrDefault(m => m.TraceId == trace.TraceId)?.EventRegistrationTimestamp.ToString("G") ?? "",
            };
            return prepareRtuStateChild;
        }

        public async Task<TreeOfAcceptableMeasParams> GetRtuAcceptableMeasParams(string username, Guid rtuId)
        {
            await Task.Delay(1);
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return null;
            return ForceAllDistancesToHaveDecimalPoint(rtu.AcceptableMeasParams);
        }

        private static TreeOfAcceptableMeasParams ForceAllDistancesToHaveDecimalPoint(TreeOfAcceptableMeasParams tree)
        {
            foreach (var branch in tree.Units)
            {
                var distances = branch.Value.Distances;
                branch.Value.Distances = new Dictionary<string, LeafOfAcceptableMeasParams>();
                foreach (var pair in distances)
                {
                    var distance = pair.Key;
                    if (!distance.Contains("."))
                        distance = pair.Key + ".0";
                    branch.Value.Distances.Add(distance, pair.Value);
                }
            }
            return tree;
        }

        /// <summary>
        /// not the same as desktop command:
        /// web client sends only id of RTU which had already been initialized and now should be RE-initialized
        /// </summary>
        /// <param name="dto">contains only RTU ID and will be filled in now (on data center)</param>
        /// <returns></returns>
        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            if (!FillIn(dto)) return new RtuInitializedDto() { ReturnCode = ReturnCode.RtuInitializationError, };
            return await _intermediateLayer.InitializeRtuAsync(dto);
        }

        private bool FillIn(InitializeRtuDto dto)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return false;
            dto.RtuMaker = rtu.RtuMaker;
            dto.RtuAddresses = new DoubleAddress()
            {
                Main = (NetAddress)rtu.MainChannel.Clone(),
                Reserve = (NetAddress)rtu.ReserveChannel.Clone(),
                HasReserveAddress = rtu.IsReserveChannelSet,
            };
            dto.IsFirstInitialization = false;
            dto.Serial = rtu.Serial;
            dto.OwnPortCount = rtu.OwnPortCount;
            dto.Children = rtu.Children;
            return true;
        }
 }
}
