using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class WcfServiceWebC2D : IWcfServiceWebC2D
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        public WcfServiceWebC2D(IMyLog logFile, Model writeModel, CurrentDatacenterParameters currentDatacenterParameters,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _currentDatacenterParameters = currentDatacenterParameters;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
        }

        public async Task<UserDto> LoginWebClient(string username, string password)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy LoginWebClient");
            await Task.Delay(1);
            try
            {
                var user = _writeModel.Users.FirstOrDefault(u => u.Title == username && UserExt.FlipFlop(u.EncodedPassword) == password);
                if (user == null)
                {
                    _logFile.AppendLine("no such user");
                    return null;
                }
                var result = new UserDto();
                result.Username = username;
                result.Role = user.Role.ToString();
                result.Zone = _writeModel.Zones.FirstOrDefault(z => z.ZoneId == user.ZoneId)?.Title;
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"{e.Message}");
                return null;
            }
        }

        public async Task<string> GetAboutInJson(string username)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetAboutInJson");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            try
            {
                var result = new AboutDto();
                result.DcSoftware = _currentDatacenterParameters.DatacenterVersion;
                result.Rtus = _writeModel.CreateAboutRtuList(user).ToList();
                _logFile.AppendLine($"Rtus contains {result.Rtus.Count} RTU");
                var resString = JsonConvert.SerializeObject(result, JsonSerializerSettings);
                return resString;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"{e.Message}");
                return "";
            }
        }

        public async Task<string> GetTreeInJson(string username)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetTreeInJson");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            try
            {
                var result = _writeModel.GetTree(_logFile, user).ToList();
                _logFile.AppendLine($"Tree contains {result.Count} RTU");
                var resString = JsonConvert.SerializeObject(result, JsonSerializerSettings);
                return resString;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"{e.Message}");
                return "";
            }
        }

        #region RTU
        public async Task<RtuInformationDto> GetRtuInformation(string username, Guid rtuId)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetTraceInformation");
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
            result.OtdrAddress = rtu.OtdrNetAddress.ToStringA();
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
            result.TraceCount = result.Children.Count(c => c.TraceState != FiberState.NotInTrace);
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
                TraceTitle = trace.Title,
                TraceState = trace.State,
                LastMeasId = _writeModel.Measurements.LastOrDefault(m => m.TraceId == trace.TraceId)?.SorFileId.ToString() ?? "",
                LastMeasTime = _writeModel.Measurements.LastOrDefault(m => m.TraceId == trace.TraceId)?.EventRegistrationTimestamp.ToString("G") ?? "",
            };
            return prepareRtuStateChild;
        }

        /// <summary>
        /// not the same as desktop command:
        /// web client sends only id of RTU which had already been initialized and now should be RE-initialized
        /// </summary>
        /// <param name="dto">contains only RTU ID and will be filled in on server</param>
        /// <returns></returns>
        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            if (!FillIn(dto)) return new RtuInitializedDto(){ReturnCode = ReturnCode.RtuInitializationError,};
            return dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.InitializeAsync(dto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.InitializeAsync(dto).Result);
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
            dto.ShouldMonitoringBeStopped = false;
            dto.Serial = rtu.Serial;
            dto.OwnPortCount = rtu.OwnPortCount;
            dto.Children = rtu.Children;
            return true;
        }
        #endregion

        #region Trace

      
        public async Task<TraceInformationDto> GetTraceInformation(string username, Guid traceId)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetTraceInformation");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var result = new TraceInformationDto();
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return result;
            result.TraceTitle = trace.Title;
            result.Port = trace.OtauPort.IsPortOnMainCharon
                ? trace.Port.ToString()
                : $"{trace.OtauPort.Serial}-{trace.OtauPort.OpticalPort}";
            result.RtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId)?.Title;

            var dict = _writeModel.BuildDictionaryByEquipmentType(trace.EquipmentIds);
            result.Equipment = TraceInfoCalculator.CalculateEquipment(dict);
            result.Nodes = TraceInfoCalculator.CalculateNodes(dict);

            result.IsLightMonitoring = trace.Mode == TraceMode.Light;
            result.Comment = trace.Comment;
            return result;
        }
        #endregion


        public async Task<OpticalEventsRequestedDto> GetOpticalEventPortion(string username, bool isCurrentEvents,
            string filterRtu = "", string filterTrace = "", string sortOrder = "desc", int pageNumber = 0, int pageSize = 100)
        {
            _logFile.AppendLine($":: WcfServiceForWebProxy GetOpticalEventList pageSize = {pageSize}  pageNumber = {pageNumber}");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var collection = isCurrentEvents ? _writeModel.ActiveMeasurements : _writeModel.Measurements;
            var sift = collection.Where(o => o.Filter(filterRtu, filterTrace, _writeModel, user)).ToList();
            return new OpticalEventsRequestedDto
            {
                FullCount = sift.Count,
                EventPortion = sift
                    .Sort(sortOrder) 
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)
                    .Select(m => m.CreateOpticalEventDto(_writeModel)).ToList()
            };
        }

        public async Task<TraceStatisticsDto> GetTraceStatistics(string username, Guid traceId, int pageNumber, int pageSize)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetTraceStatistics");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var result = new TraceStatisticsDto();
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return result;
            result.TraceTitle = trace.Title;
            result.Port = trace.OtauPort.IsPortOnMainCharon
                ? trace.Port.ToString()
                : $"{trace.OtauPort.Serial}-{trace.OtauPort.OpticalPort}";
            result.RtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId)?.Title;
            result.BaseRefs = _writeModel.BaseRefs
                .Where(b => b.TraceId == traceId)
                .Select(l => new BaseRefInfoDto()
                {
                    SorFileId = l.SorFileId,
                    BaseRefType = l.BaseRefType,
                    AssignmentTimestamp = l.SaveTimestamp,
                    Username = l.UserName,
                }).ToList();
            var sift = _writeModel.Measurements.Where(m => m.TraceId == traceId).ToList();
            result.MeasFullCount = sift.Count;
            result.MeasPortion = sift
                .OrderByDescending(e => e.EventRegistrationTimestamp)
                .Skip(pageNumber*pageSize)
                .Take(pageSize)
                .Select(l => new MeasurementDto()
                {
                    SorFileId = l.SorFileId,
                    BaseRefType = l.BaseRefType,
                    EventRegistrationTimestamp = l.EventRegistrationTimestamp,
                    IsEvent = l.EventStatus > EventStatus.JustMeasurementNotAnEvent,
                    TraceState = l.TraceState,
                }).ToList();
            return result;
        }
    }
}