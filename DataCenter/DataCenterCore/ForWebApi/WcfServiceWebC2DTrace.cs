using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Newtonsoft.Json.Linq;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceWebC2D
    {
        public async Task<TraceInformationDto> GetTraceInformation(string username, Guid traceId)
        {
            if (!await Authorize(username, "GetTraceInformation")) return null;

            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return null;

            var result = new TraceInformationDto { Header = BuildHeader(trace) };

            var dict = _writeModel.BuildDictionaryByEquipmentType(trace.EquipmentIds);
            result.Equipment = TraceInfoCalculator.CalculateEquipment(dict);
            result.Nodes = TraceInfoCalculator.CalculateNodes(dict);

            result.IsLightMonitoring = trace.Mode == TraceMode.Light;
            result.Comment = trace.Comment;
            return result;
        }

        public async Task<TraceStatisticsDto> GetTraceStatistics(string username, Guid traceId, int pageNumber, int pageSize)
        {
            if (!await Authorize(username, "GetTraceStatistics")) return null;

            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return null;

            var result = new TraceStatisticsDto
            {
                Header = BuildHeader(trace),
                BaseRefs = _writeModel.BaseRefs
                    .Where(b => b.TraceId == traceId)
                    .Select(l => new BaseRefInfoDto()
                    {
                        SorFileId = l.SorFileId,
                        BaseRefType = l.BaseRefType,
                        AssignmentTimestamp = l.SaveTimestamp,
                        Username = l.UserName,
                    }).ToList()
            };

            var sift = _writeModel.Measurements.Where(m => m.TraceId == traceId).ToList();
            result.MeasFullCount = sift.Count;
            result.MeasPortion = sift
                .OrderByDescending(e => e.EventRegistrationTimestamp)
                .Skip(pageNumber * pageSize)
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

        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();

        public async Task<TraceStateDto> GetTraceState(string username, string requestBody)
        {
            if (!await Authorize(username, "GetTraceState")) return null;

            TraceHeaderDto header = null;
            var measurement = GetRequestedMeasurement(requestBody, ref header);
            if (measurement == null) return null;

            var dto = Mapper.Map<TraceStateDto>(measurement);
            dto.Header = header;
            if (measurement.Accidents != null)
                dto.Accidents = F(measurement.Accidents).ToList();


            return dto;
        }

        private Measurement GetRequestedMeasurement(string requestBody, ref TraceHeaderDto header)
        {
            dynamic data = JObject.Parse(requestBody);
            var requestType = data["type"].ToObject<string>();
            return requestType == "traceId" 
                ? (Measurement) GetMeasByTraceId(data["traceId"].ToObject<Guid>(), ref header) 
                : (Measurement) GetMeasBySorFileId(data["fileId"].ToObject<int>(), ref header);
        }

        private Measurement GetMeasByTraceId(Guid traceId, ref TraceHeaderDto header)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return null;

            header = BuildHeader(trace);
            return _writeModel.Measurements.LastOrDefault(m => m.TraceId == traceId);
        }

        private Measurement GetMeasBySorFileId(int sorFileId, ref TraceHeaderDto header)
        {
            var measurement = _writeModel.Measurements.LastOrDefault(m => m.SorFileId == sorFileId);
            if (measurement == null)
                return null;
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == measurement.TraceId);
            if (trace == null)
                return null;

            header = BuildHeader(trace);
            return measurement;
        }

        private IEnumerable<AccidentLineDto> F(List<AccidentOnTraceV2> accidents)
        {
            int i = 1;
            foreach (var accidentOnTraceV2 in accidents)
            {
                var line = _accidentLineModelFactory.Create(accidentOnTraceV2, i++, true, GpsInputMode.Degrees, false);
                var dtoLine = Mapper.Map<AccidentLineDto>(line);
                yield return dtoLine;
            }
        }

        private async Task<bool> Authorize(string username, string log)
        {
            await Task.Delay(1);
            _logFile.AppendLine($"WebApi::{log}");
            if (_writeModel.Users.Any(u => u.Title == username))
                return true;
            _logFile.AppendLine("Not authorized access");
            return false;
        }

        private TraceHeaderDto BuildHeader(Trace trace)
        {
            var result = new TraceHeaderDto();
            result.TraceTitle = trace.Title;
            result.Port = trace.Port < 1 ? "-1" :
                trace.OtauPort.IsPortOnMainCharon
                    ? trace.Port.ToString()
                    : $"{trace.OtauPort.Serial}-{trace.OtauPort.OpticalPort}";
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            result.RtuTitle = rtu?.Title;
            result.RtuVersion = rtu?.Version;
            return result;
        }
    }

}
