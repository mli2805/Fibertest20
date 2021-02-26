using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public static class TraceWebDtoFactory
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();

        public static TraceStateDto GetTraceStateDto(this Model writeModel,
            AccidentLineModelFactory accidentLineModelFactory, int sorFileId)
        {
            var measurement = writeModel.Measurements.LastOrDefault(m => m.SorFileId == sorFileId);
            return measurement == null ? null : writeModel.GetTraceStateDto(accidentLineModelFactory, measurement);
        }

        public static TraceStateDto GetTraceStateDto(this Model writeModel,
            AccidentLineModelFactory accidentLineModelFactory, Guid traceId)
        {
            var trace = writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return null;

            var measurement =  writeModel.Measurements.LastOrDefault(m => m.TraceId == traceId);
            return measurement == null ? null : writeModel.GetTraceStateDto(accidentLineModelFactory, measurement);
        }

        private static TraceStateDto GetTraceStateDto(this Model writeModel, 
            AccidentLineModelFactory accidentLineModelFactory, Measurement measurement)
        {
            var trace = writeModel.Traces.FirstOrDefault(t => t.TraceId == measurement.TraceId);
            if (trace == null)
                return null;

            var dto = Mapper.Map<TraceStateDto>(measurement);
            dto.Header = BuildHeader(writeModel, trace);
            dto.Accidents = BuildAccidents(measurement.Accidents, accidentLineModelFactory).ToList();
           
            return dto;
        }

        public static TraceHeaderDto BuildHeader(this Model writeModel, Trace trace)
        {
            var result = new TraceHeaderDto();
            result.TraceTitle = trace.Title;
            result.Port = trace.Port < 1 ? "-1" :
                trace.OtauPort.IsPortOnMainCharon
                    ? trace.Port.ToString()
                    : $"{trace.OtauPort.Serial}-{trace.OtauPort.OpticalPort}";
            var rtu = writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            result.RtuTitle = rtu?.Title;
            result.RtuVersion = rtu?.Version;
            return result;
        }

        private static IEnumerable<AccidentLineDto> BuildAccidents(List<AccidentOnTraceV2> accidents,
            AccidentLineModelFactory accidentLineModelFactory)
        {
            int i = 1;
            foreach (var accidentOnTraceV2 in accidents)
            {
                var line = accidentLineModelFactory.Create(accidentOnTraceV2, i++, true, GpsInputMode.Degrees, false);
                var dtoLine = Mapper.Map<AccidentLineDto>(line);
                if (line.Position != null)
                    dtoLine.Position = new GeoPoint{Latitude = line.Position.Value.Lat, Longitude = line.Position.Value.Lng};
                yield return dtoLine;
            }
        }

    }
}