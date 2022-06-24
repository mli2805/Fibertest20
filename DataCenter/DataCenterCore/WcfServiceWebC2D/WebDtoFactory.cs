using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public static class WebDtoFactory
    {
        public static IEnumerable<RtuDto> GetTree(this Model writeModel, IMyLog logFile, User user)
        {
            foreach (var rtu in writeModel.Rtus)
            {
                if (!rtu.ZoneIds.Contains(user.ZoneId))
                    continue;
                var rtuDto = rtu.CreateRtuDto();
                for (int i = 1; i <= rtuDto.OwnPortCount; i++)
                {
                    rtuDto.Children.Add(rtu.GetChildForPort(i, writeModel, logFile, user));
                }
                //detached traces
                foreach (var trace in writeModel.Traces.Where(t => t.RtuId == rtu.Id && t.Port == -1))
                {
                    rtuDto.Children.Add(trace.CreateTraceDto());
                }

                yield return rtuDto;
            }
        }

        public static IEnumerable<AboutRtuDto> CreateAboutRtuList(this Model writeModel, User user)
        {
            foreach (var rtu in writeModel.Rtus)
            {
                if (!rtu.ZoneIds.Contains(user.ZoneId))
                    continue;
                yield return rtu.CreateAboutRtuDto();
            }
        }

        private static AboutRtuDto CreateAboutRtuDto(this Rtu r)
        {
            return new AboutRtuDto()
            {
                Title = r.Title,
                Model = r.Mfid,
                Serial = r.Serial,
                Version = r.Version,
            };
        }

        private static RtuDto CreateRtuDto(this Rtu r)
        {
            return new RtuDto()
            {
                RtuId = r.Id,
                Title = r.Title,
                RtuMaker = r.RtuMaker,
                Serial = r.Serial,
                MainVeexOtau = r.MainVeexOtau,
                OtdrId = r.OtdrId,

                FullPortCount = r.FullPortCount,
                OwnPortCount = r.OwnPortCount,

                MainChannel = r.MainChannel.Clone(),
                MainChannelState = r.MainChannelState,
                ReserveChannel = r.ReserveChannel.Clone(),
                ReserveChannelState = r.ReserveChannelState,
                IsReserveChannelSet = r.IsReserveChannelSet,
                OtdrNetAddress = r.OtdrNetAddress.Clone(),
                BopState = r.BopState,

                MonitoringMode = r.MonitoringState,
                Version = r.Version,
                Version2 = r.Version2,
            };
        }

        private static ChildDto GetChildForPort(this Rtu rtu, int port, Model writeModel, IMyLog logFile, User user)
        {
            if (rtu.Children.ContainsKey(port))
            {
                var otau = writeModel.Otaus.FirstOrDefault(o => o.NetAddress?.Ip4Address == rtu.Children[port].NetAddress.Ip4Address);
                if (otau == null)
                {
                    logFile.AppendLine($"Something strange happened on RTU {rtu.Title} port {port}: otau not found");
                    return null;
                }
                var otauWebDto = otau.CreateOtauWebDto(port);
                for (var j = 1; j <= otau.PortCount; j++)
                {
                    var traceOnOtau = writeModel.Traces.FirstOrDefault(t => t.RtuId == rtu.Id
                                                                            && t.OtauPort != null
                                                                            && t.OtauPort.Serial == otau.Serial
                                                                            && t.OtauPort.OpticalPort == j
                                                                            && t.ZoneIds.Contains(user.ZoneId));
                    otauWebDto.Children.Add(traceOnOtau != null
                        ? traceOnOtau.CreateTraceDto()
                        : new ChildDto(ChildType.FreePort) { Port = j });
                }
                return otauWebDto;
            }

            var trace = writeModel.Traces.FirstOrDefault(t => t.RtuId == rtu.Id
                                                              && t.OtauPort != null
                                                              && t.OtauPort.IsPortOnMainCharon
                                                              && t.Port == port
                                                              && t.ZoneIds.Contains(user.ZoneId));
            return trace != null
                ? trace.CreateTraceDto()
                : new ChildDto(ChildType.FreePort) { Port = port };
        }

        private static TraceDto CreateTraceDto(this Trace t)
        {
            return new TraceDto(ChildType.Trace)
            {
                TraceId = t.TraceId,
                RtuId = t.RtuId,
                Title = t.Title,
                OtauPort = t.OtauPort,
                IsAttached = t.IsAttached,
                Port = t.Port,
                State = t.State,
                HasEnoughBaseRefsToPerformMonitoring = t.HasEnoughBaseRefsToPerformMonitoring,
                IsIncludedInMonitoringCycle = t.IsIncludedInMonitoringCycle,
            };
        }

        private static OtauWebDto CreateOtauWebDto(this Otau o, int port)
        {
            return new OtauWebDto(ChildType.Otau)
            {
                OtauId = o.Id,
                RtuId = o.RtuId,
                OtauNetAddress = o.NetAddress,
                IsOk = o.IsOk,
                Serial = o.Serial,

                Port = port,
            };
        }

        public static OpticalEventDto CreateOpticalEventDto(this Measurement m, Model writeModel)
        {
            return new OpticalEventDto()
            {
                EventId = m.SorFileId,
                RtuTitle = writeModel.Rtus.FirstOrDefault(r => r.Id == m.RtuId)?.Title,
                TraceTitle = writeModel.Traces.FirstOrDefault(t => t.TraceId == m.TraceId)?.Title,
                TraceState = m.TraceState,
                BaseRefType = m.BaseRefType,
                EventRegistrationTimestamp = m.EventRegistrationTimestamp,
                MeasurementTimestamp = m.MeasurementTimestamp,

                EventStatus = m.EventStatus,
                StatusChangedTimestamp = m.StatusChangedTimestamp,
                StatusChangedByUser = m.StatusChangedByUser,

                Comment = m.Comment,
            };
        }

        public static OpticalAlarm CreateOpticalAlarm(this Measurement m)
        {
            return new OpticalAlarm()
            {
                SorFileId = m.SorFileId,
                TraceId = m.TraceId,
                HasBeenSeen = true,
            };
        }

        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();

        public static NetworkEventDto CreateNetworkEventDto(this NetworkEvent n, Model writeModel)
        {
            var dto = Mapper.Map<NetworkEventDto>(n);
            dto.RtuTitle = writeModel.Rtus.FirstOrDefault(r => r.Id == n.RtuId)?.Title;
            return dto;
        }

        public static BopEventDto CreateBopEventDto(this BopNetworkEvent n, Model writeModel)
        {
            var dto = Mapper.Map<BopEventDto>(n);
            dto.RtuTitle = writeModel.Rtus.FirstOrDefault(r => r.Id == n.RtuId)?.Title;
            return dto;
        }

        public static IEnumerable<NetworkAlarm> CreateNetworkAlarms(this NetworkEvent n)
        {
            var na = new NetworkAlarm { EventId = n.Ordinal, RtuId = n.RtuId, HasBeenSeen = true };
            if (n.OnMainChannel == ChannelEvent.Broken)
            {
                na.Channel = "Main";
                yield return na;
            }
            if (n.OnReserveChannel == ChannelEvent.Broken)
            {
                na.Channel = "Reserve";
                yield return na;
            }
        }

        public static BopAlarm CreateBopAlarm(this BopNetworkEvent b)
        {
            return new BopAlarm() { EventId = b.Ordinal, Serial = b.Serial, HasBeenSeen = true };
        }
    }
}
