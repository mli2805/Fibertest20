﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceWebC2D
    {
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
                            : new RtuMonitoringPortDto() { Port = $"{i}-{j}", PortMonitoringMode = PortMonitoringMode.NoTraceJoined });
                    }
                }
                else
                {
                    var trace = _writeModel.Traces.FirstOrDefault(t =>
                        t.RtuId == rtu.Id && t.Port == i && (t.OtauPort == null || t.OtauPort.IsPortOnMainCharon));
                    result.Add(trace != null
                        ? PrepareRtuMonitoringPortLine(trace, i, "")
                        : new RtuMonitoringPortDto() { Port = i.ToString(), PortMonitoringMode = PortMonitoringMode.NoTraceJoined });
                }
            }
            return result;
        }

        private RtuMonitoringPortDto PrepareRtuMonitoringPortLine(Trace trace, int port, string mainPort)
        {
            var result = new RtuMonitoringPortDto()
            {
                Port = mainPort + port,
                TraceId = trace.TraceId.ToString(),
                OtauPortDto = trace.OtauPort,
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

//        public async Task<MonitoringSettingsAppliedDto> PostRtuMonitoringSettings(string username, Guid rtuId, RtuMonitoringSettingsDto dto)
//        {
//            _logFile.AppendLine(":: WcfServiceForWebProxy GetRtuMonitoringSettings");
//            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
//            if (user == null)
//            {
//                _logFile.AppendLine("Not authorized access");
//                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };
//            }
//
//            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
//            if (rtu == null)
//            {
//                _logFile.AppendLine("RTU not found");
//                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };
//            }
//
//            var dto2 = Map(rtuId, dto);
//
//            var result = rtu.RtuMaker == RtuMaker.VeEX
//                ? await _clientToRtuVeexTransmitter.ApplyMonitoringSettingsAsync(dto2)
//                : await _clientToRtuTransmitter.ApplyMonitoringSettingsAsync(dto2);
//            return result;
//        }

//        private ApplyMonitoringSettingsDto Map(Guid rtuId, RtuMonitoringSettingsDto dto)
//        {
//            var applyMonitoringSettingsDto = new ApplyMonitoringSettingsDto()
//            {
//                RtuId = rtuId,
//                IsMonitoringOn = dto.MonitoringMode == MonitoringState.On,
//
//                Timespans = new MonitoringTimespansDto()
//                {
//                    FastSave = dto.FastSave.GetTimeSpan(),
//                    PreciseMeas = dto.PreciseMeas.GetTimeSpan(),
//                    PreciseSave = dto.PreciseSave.GetTimeSpan(),
//                },
//
//                Ports = new List<PortWithTraceDto>(),
//            };
//            foreach (var line in dto.Lines.Where(l=>l.PortMonitoringMode == PortMonitoringMode.On))
//            {
//                var traceGuid = Guid.Parse(line.TraceId);
//                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceGuid);
//                if (trace == null)
//                    continue;
//                var portWithTraceDto = new PortWithTraceDto()
//                {
//                    TraceId = traceGuid,
//                    OtauPort = trace.OtauPort,
//                };
//                applyMonitoringSettingsDto.Ports.Add(portWithTraceDto);
//            }
//
//            return applyMonitoringSettingsDto;
//        }
    }
}