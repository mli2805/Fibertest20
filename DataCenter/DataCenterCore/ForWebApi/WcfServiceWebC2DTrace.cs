using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceWebC2D
    {
        public async Task<TraceInformationDto> GetTraceInformation(string username, Guid traceId)
        {
            var trace = await GetTrace(username, traceId, "GetTraceInformation");
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

            var trace = await GetTrace(username, traceId, "GetTraceStatistics");
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
            cfg => cfg.AddProfile<MappingToDtoProfile>()).CreateMapper();

        public async Task<TraceStateDto> GetTraceState(string username, Guid traceId)
        {
            var trace = await GetTrace(username, traceId, "GetTraceState");
            if (trace == null)
                return null;

            var lastMeas = _writeModel.Measurements.LastOrDefault(m => m.TraceId == traceId);
            if (lastMeas == null)
                return null;

            var dto = Mapper.Map<TraceStateDto>(lastMeas);
            dto.Header = BuildHeader(trace);
            if (lastMeas.Accidents != null)
                dto.Accidents = F(lastMeas.Accidents).ToList();

            
            return dto;
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

        private async Task<Trace> GetTrace(string username, Guid traceId, string log)
        {
            await Task.Delay(1);
            _logFile.AppendLine($"WebApi::{log}");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }

            return _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
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

    public class MappingToDtoProfile : Profile
    {
        public MappingToDtoProfile()
        {
            CreateMap<PointLatLng, GeoPoint>();
            CreateMap<AccidentLineModel, AccidentLineDto>();
            CreateMap<Measurement, TraceStateDto>()
                .ForMember(dest => dest.RegistrationTimestamp, opt => opt.MapFrom(src => src.EventRegistrationTimestamp))
                .ForMember(dest=> dest.Accidents, opt => opt.Ignore());
        }
    }
}
