using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json.Linq;
using Optixsoft.SorExaminer.OtdrDataFormat;

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

            var result = new TraceInformationDto { Header = _writeModel.BuildHeader(trace) };

            var dict = _writeModel.BuildDictionaryByEquipmentType(trace.EquipmentIds);
            result.Equipment = TraceInfoCalculator.CalculateEquipmentForWeb(dict);
            result.Nodes = TraceInfoCalculator.CalculateNodesForWeb(dict);

            result.IsLightMonitoring = trace.Mode == TraceMode.Light;
            result.Comment = trace.Comment;
            return result;
        }

        public async Task<TraceLandmarksDto> GetTraceLandmarks(string username, Guid traceId)
        {
            if (!await Authorize(username, "GetTraceLandmarks")) return null;

            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return null;

            _logFile.AppendLine("We are going to get landmarks...");
            var landmarks = trace.PreciseId != Guid.Empty
                ? _landmarksBaseParser.GetLandmarks(await GetBase(trace.PreciseId), trace)
                : _landmarksGraphParser.GetLandmarks(trace);

            return new TraceLandmarksDto()
            {
                Header = _writeModel.BuildHeader(trace),
                Landmarks = landmarks.Select(MapLm).ToList(),
            };
        }

        private async Task<OtdrDataKnownBlocks> GetBase(Guid baseId)
        {
            if (baseId == Guid.Empty)
                return null;

            var baseRef = _writeModel.BaseRefs.First(b => b.Id == baseId);
            var sorBytes = await _sorFileRepository.GetSorBytesAsync(baseRef.SorFileId);
            return SorData.FromBytes(sorBytes);
        }

        private LandmarkDto MapLm(Landmark lm)
        {
            return new LandmarkDto()
            {
                Ordinal = lm.Number,
                NodeTitle = lm.NodeTitle,
                EqType = lm.EquipmentType,
                EquipmentTitle = lm.EquipmentTitle,
                DistanceKm = lm.OpticalDistance,
                EventOrdinal = lm.EventNumber,
                Coors = new GeoPoint()
                {
                    Latitude = lm.GpsCoors.Lat,
                    Longitude = lm.GpsCoors.Lng,
                }
            };
        }

        public async Task<TraceStatisticsDto> GetTraceStatistics(string username, Guid traceId, int pageNumber, int pageSize)
        {
            if (!await Authorize(username, "GetTraceStatistics")) return null;

            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return null;

            var result = new TraceStatisticsDto
            {
                Header = _writeModel.BuildHeader(trace),
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

        public async Task<TraceStateDto> GetTraceState(string username, string requestBody)
        {
            if (!await Authorize(username, "GetTraceState")) return null;

            dynamic data = JObject.Parse(requestBody);
            var requestType = data["type"].ToObject<string>();
            return TraceWebDtoFactory.GetTraceStateDto(_writeModel, _accidentLineModelFactory, 
                requestType != "traceId" 
                    ? data["fileId"].ToObject<int>() 
                    : data["traceId"].ToObject<Guid>());
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

        public async Task<BaseRefAssignedDto> AssignBaseRefs(AssignBaseRefsDto dto)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == dto.TraceId);
            if (trace == null)
                return new BaseRefAssignedDto()
                    { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = "trace not found" };

            var precise = dto.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (precise != null && trace.PreciseId != Guid.Empty)
                dto.DeleteOldSorFileIds.Add(_writeModel.BaseRefs.First(b => b.Id == trace.PreciseId).SorFileId);

            var fast = dto.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Fast);
            if (fast != null && trace.FastId != Guid.Empty)
                dto.DeleteOldSorFileIds.Add(_writeModel.BaseRefs.First(b=>b.Id == trace.FastId).SorFileId);

            var additional = dto.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Additional);
            if (additional != null && trace.AdditionalId != Guid.Empty)
                dto.DeleteOldSorFileIds.Add(_writeModel.BaseRefs.First(b=>b.Id == trace.AdditionalId).SorFileId);

            return await _wcfIntermediate.AssignBaseRefAsync(dto);
        }
    }

}
