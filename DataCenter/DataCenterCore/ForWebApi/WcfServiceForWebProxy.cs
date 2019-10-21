﻿using System;
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

        public async Task<string> GetTreeInJson()
        {
            await Task.Delay(1);
            _logFile.AppendLine(":: WcfServiceForWebProxy GetTreeInJson");
            try
            {
                var result = _writeModel.CreateTree(_logFile).ToList();
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

        public async Task<List<RtuDto>> GetRtuList()
        {
            try
            {
                await Task.Delay(1);
                _logFile.AppendLine(":: WcfServiceForWebProxy GetRtuList");
                var result = _writeModel.CreateTree(_logFile).ToList();
                _logFile.AppendLine($"Tree contains {result.Count} RTU");
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetRtuList: {e.Message}");
                return new List<RtuDto>();
            }
        }

        public async Task<List<TraceDto>> GetTraceList()
        {
            await Task.Delay(1);
            return _writeModel.Traces.Select(t => t.CreateTraceDto()).ToList();
        }

        public async Task<List<OpticalEventDto>> GetOpticalEventList()
        {
            await Task.Delay(1);
            _logFile.AppendLine(":: WcfServiceForWebProxy GetOpticalEventList");
            return _writeModel.Measurements
                .Where(m => m.EventStatus > EventStatus.JustMeasurementNotAnEvent)
                .Select(m => m.CreateOpticalEventDto(_writeModel)).ToList();
        }

        public async Task<TraceStatisticsDto> GetTraceStatistics(Guid traceId)
        {
            await Task.Delay(1);
            _logFile.AppendLine(":: WcfServiceForWebProxy GetTraceStatistics");

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
                    BaseRefAssignmentTime = l.SaveTimestamp,
                    Username = l.UserName,
                }).ToList();
            result.Measurements = _writeModel.Measurements
                .Where(m => m.TraceId == traceId)
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