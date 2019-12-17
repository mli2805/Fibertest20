using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForWebProxyInterface;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForWebProxy : IWcfServiceForWebProxy
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IMyLog _logFile;
        private readonly Model _writeModel;

        public WcfServiceForWebProxy(IMyLog logFile, Model writeModel)
        {
            _logFile = logFile;
            _writeModel = writeModel;
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
                var result = _writeModel.CreateTree(_logFile, user).ToList();
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
                                   : new RtuStateChildDto() {Port = $"{i}-{j}", TraceState = FiberState.Nothing});
                    }
                }
                else 
                {
                    var trace = _writeModel.Traces.FirstOrDefault(t => 
                        t.RtuId == rtu.Id  && t.Port == i && (t.OtauPort == null || t.OtauPort.IsPortOnMainCharon));
                    result.Add(trace != null
                        ? PrepareRtuStateChild(trace, i, "")
                        : new RtuStateChildDto() {Port = i.ToString(), TraceState = FiberState.Nothing});
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
                LastMeasId = _writeModel.Measurements.LastOrDefault(m=>m.TraceId == trace.TraceId)?.SorFileId.ToString() ?? "",
                LastMeasTime = _writeModel.Measurements.LastOrDefault(m=>m.TraceId == trace.TraceId)?.EventRegistrationTimestamp.ToString("G") ?? "",
            };
            return prepareRtuStateChild;
        }
        
        public async Task<RtuMonitoringSettingsDto> GetRtuMonitoringSettings(string username, Guid rtuId)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetRtuMonitoringSettings");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var result = new RtuMonitoringSettingsDto();
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return result;
            result.RtuTitle = rtu.Title;
            result.MonitoringMode = rtu.MonitoringState;
            result.PreciseMeas = rtu.PreciseMeas;
            result.PreciseSave = rtu.PreciseSave;
            result.FastSave = rtu.FastSave;
            result.Lines = PrepareRtuMonitoringPortLines(rtu);
            return result;
        }

        private List<RtuMonitoringPortDto> PrepareRtuMonitoringPortLines(Rtu rtu)
        {
            var result = new List<RtuMonitoringPortDto>();
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
                            ? PrepareRtuMonitoringPortLine(trace, j, $"{i}-")
                            : new RtuMonitoringPortDto() {Port = $"{i}-{j}", PortMonitoringMode = PortMonitoringMode.NoTraceJoined});
                    }
                }
                else 
                {
                    var trace = _writeModel.Traces.FirstOrDefault(t => 
                        t.RtuId == rtu.Id  && t.Port == i && (t.OtauPort == null || t.OtauPort.IsPortOnMainCharon));
                    result.Add(trace != null
                        ? PrepareRtuMonitoringPortLine(trace, i, "")
                        : new RtuMonitoringPortDto() {Port = i.ToString(), PortMonitoringMode = PortMonitoringMode.NoTraceJoined});
                }
            }
            return result;
        }

        private RtuMonitoringPortDto PrepareRtuMonitoringPortLine(Trace trace, int port, string mainPort)
        {
            var result = new RtuMonitoringPortDto()
            {
                Port = mainPort + port,
                TraceTitle = trace.Title,
                DurationOfFastBase = trace.FastDuration.Seconds,
                DurationOfPreciseBase = trace.PreciseDuration.Seconds,
                PortMonitoringMode = !trace.HasEnoughBaseRefsToPerformMonitoring 
                    ? PortMonitoringMode.TraceHasNoBase
                    : !trace.IsIncludedInMonitoringCycle
                        ? PortMonitoringMode.Off
                        : PortMonitoringMode.On,
            };
            return result;
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


        public async Task<List<OpticalEventDto>> GetOpticalEventList(string username, bool isCurrentEvents, string filterRtu = "",
            string filterTrace = "", string sortOrder = "desc", int pageNumber = 0, int pageSize = 100)
        {
            _logFile.AppendLine($":: WcfServiceForWebProxy GetOpticalEventList pageSize = {pageSize}  pageNumber = {pageNumber}");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);


            return isCurrentEvents ?
                _writeModel.ActiveMeasurements
                .Where(m => m.Filter(filterRtu, filterTrace, _writeModel, user))
                .Sort(sortOrder)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .Select(m => m.CreateOpticalEventDto(_writeModel)).ToList() 
                :
                _writeModel.Measurements
                .Where(m => m.Filter(filterRtu, filterTrace, _writeModel, user))
                .Sort(sortOrder)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .Select(m => m.CreateOpticalEventDto(_writeModel)).ToList();
        }

        public async Task<TraceStatisticsDto> GetTraceStatistics(string username, Guid traceId)
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
            result.Measurements = _writeModel.Measurements
                .Where(m => m.TraceId == traceId)
                .OrderByDescending(e => e.EventRegistrationTimestamp)
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

    public static class MeasExt
    {
        public static bool Filter(this Measurement measurement, string filterRtu, string filterTrace, Model writeModel, User user)
        {
            if (measurement.EventStatus == EventStatus.JustMeasurementNotAnEvent)
                return false;

            var rtu = writeModel.Rtus.FirstOrDefault(r => r.Id == measurement.RtuId);
            if (rtu == null 
                || !rtu.ZoneIds.Contains(user.ZoneId)
                || (!string.IsNullOrEmpty(filterRtu) && !rtu.Title.Contains(filterRtu)))
            {
                    return false;
            }

            var trace = writeModel.Traces.FirstOrDefault(t => t.TraceId == measurement.TraceId);
            if (trace == null
                || !trace.ZoneIds.Contains(user.ZoneId)
                || (!string.IsNullOrEmpty(filterTrace) && !trace.Title.Contains(filterTrace)))
            {
                return false;
            }
            return true;
        }
        public static IEnumerable<Measurement> Sort(this IEnumerable<Measurement> input, string param)
        {
            return param == "asc" ? input.OrderBy(o => o.SorFileId) : input.OrderByDescending(o => o.SorFileId);
        }

    }
}