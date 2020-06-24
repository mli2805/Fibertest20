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
        private readonly AccidentLineModelFactory _accidentLineModelFactory;

        public WcfServiceWebC2D(IMyLog logFile, Model writeModel, CurrentDatacenterParameters currentDatacenterParameters,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter,
            AccidentLineModelFactory accidentLineModelFactory)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _currentDatacenterParameters = currentDatacenterParameters;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
            _accidentLineModelFactory = accidentLineModelFactory;
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

        public async Task<string> GetCurrentAccidents(string username)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetCurrentAccidents");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }

            try
            {
                var result = new AlarmsDto
                {
                    NetworkAlarms = await GetCurrentNetworkEvents(username),
                    OpticalAlarms = await GetCurrentOpticalEvents(username),
                    BopAlarms = new List<BopAlarm>()
                };
                _logFile.AppendLine($"dto contains {result.NetworkAlarms.Count} network alarms, {result.OpticalAlarms.Count} optical alarms and {result.BopAlarms.Count} bop alarms");
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
        /// <param name="dto">contains only RTU ID and will be filled in on server</param>
        /// <returns></returns>
        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            if (!FillIn(dto)) return new RtuInitializedDto() { ReturnCode = ReturnCode.RtuInitializationError, };
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

        public async Task<AssignBaseParamsDto> GetAssignBaseParams(string username, Guid traceId)
        {
            if (!await Authorize(username, "GetAssignBaseParams")) return null;

            var result = new AssignBaseParamsDto();
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null) return result;
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null) return result;
            result.RtuTitle = rtu.Title;
            result.OtdrId = rtu.OtdrId;
            result.PreciseId = trace.PreciseId;
            result.FastId = trace.FastId;
            result.AdditionalId = trace.AdditionalId;
            return result;
        }


        public async Task<OpticalEventsRequestedDto> GetOpticalEventPortion(string username, bool isCurrentEvents,
            string filterRtu = "", string filterTrace = "", string sortOrder = "desc", int pageNumber = 0, int pageSize = 100)
        {
            if (!await Authorize(username, "GetOpticalEventPortion")) return null;

            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
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
        public async Task<List<OpticalAlarm>> GetCurrentOpticalEvents(string username)
        {
            if (!await Authorize(username, "GetCurrentOpticalEvents")) return null;

            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            var sift = _writeModel.ActiveMeasurements.Where(o => o.Filter(null, null, _writeModel, user)).ToList();
            return sift.Select(m => m.CreateOpticalAlarm()).ToList();
        }

        public async Task<NetworkEventsRequestedDto> GetNetworkEventPortion(string username, bool isCurrentEvents,
            string filterRtu, string sortOrder, int pageNumber, int pageSize)
        {
            if (!await Authorize(username, "GetNetworkEventPortion")) return null;

            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            var sift = isCurrentEvents
                ? _writeModel.Rtus
                    .Where(r => r.Filter(user, filterRtu))
                    .Select(rtu => _writeModel.NetworkEvents.LastOrDefault(n => n.RtuId == rtu.Id))
                    .Where(lastNetworkEvent => lastNetworkEvent != null).ToList()
                : _writeModel.NetworkEvents
                    .Where(n => n.Filter(filterRtu, _writeModel, user)).ToList();
            return new NetworkEventsRequestedDto
            {
                FullCount = sift.Count,
                EventPortion = sift
                    .Sort(sortOrder)
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)
                    .Select(m => m.CreateNetworkEventDto(_writeModel)).ToList()
            };
        }

        public async Task<List<NetworkAlarm>> GetCurrentNetworkEvents(string username)
        {
            if (!await Authorize(username, "GetCurrentNetworkEvents")) return null;

            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            var sift = _writeModel.Rtus
                .Where(r => r.Filter(user, null))
                .Select(rtu => _writeModel.NetworkEvents.LastOrDefault(n => n.RtuId == rtu.Id))
                .Where(lastNetworkEvent => lastNetworkEvent != null).ToList();
            var result = new List<NetworkAlarm>();
            foreach (var networkEvent in sift)
                result.AddRange(networkEvent.CreateNetworkAlarms());
            return result;
        }

    }
}