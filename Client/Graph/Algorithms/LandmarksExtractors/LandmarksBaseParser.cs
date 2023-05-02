using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Graph
{
    public class LandmarksBaseParser
    {
        private readonly Model _readModel;

        public LandmarksBaseParser(Model readModel)
        {
            _readModel = readModel;
        }

        // web client requests landmarks for trace, calculated on DC
        public List<Landmark> GetLandmarksFromBaseRef(OtdrDataKnownBlocks sorData, Trace trace)
        {
            var modelWithoutAdjustmentPoint = _readModel
                .GetTraceComponentsByIds(trace)
                .ReCalculateGpsDistancesForTraceModel()
                .ExcludeAdjustmentPoints();
            return GetLandmarksInner(sorData, modelWithoutAdjustmentPoint);
        }

        // LandmarksView in desktop client
        // already has traceModel - recalculate distances and returns landmarks
        public List<Landmark> GetLandmarks(OtdrDataKnownBlocks sorData,
            TraceModelForBaseRef modelIncludingAdjustmentPoints)
        {
            var modelWithoutAdjustmentPoint = modelIncludingAdjustmentPoints
                .ReCalculateGpsDistancesForTraceModel()
                .ExcludeAdjustmentPoints();
            return GetLandmarksInner(sorData, modelWithoutAdjustmentPoint);
        }

        private List<Landmark> GetLandmarksInner(OtdrDataKnownBlocks sorData, TraceModelForBaseRef modelWithoutAdjustmentPoint)
        {
            var gpsDistance = 0.0;
            var result = new List<Landmark>();

            var rtu = _readModel.Rtus.First(r => r.NodeId == modelWithoutAdjustmentPoint.NodeArray[0].NodeId);

            var linkParameters = sorData.LinkParameters;
            var prevOwt = linkParameters.LandmarkBlocks[0].Location;
            for (int i = 0; i < linkParameters.LandmarksCount; i++)
            {
                var sorLandmark = linkParameters.LandmarkBlocks[i];
                var equipmentId = modelWithoutAdjustmentPoint.EquipArray[i].EquipmentId;
                var equipment = i > 0 ? modelWithoutAdjustmentPoint.EquipArray[i] : null;
                var fiber = i > 0 ? modelWithoutAdjustmentPoint.FiberArray[i - 1] : null; 
                var section = fiber == null
                    ? 0
                    : fiber.UserInputedLength > 0
                        ? fiber.UserInputedLength / 1000
                        : Math.Round(modelWithoutAdjustmentPoint.DistancesMm[i - 1] / 1_000_000.0, 3);
                gpsDistance += section;
                var comment = i == 0
                    ? _readModel.Rtus.First(r => r.NodeId == modelWithoutAdjustmentPoint.NodeArray[0].NodeId).Comment
                    : modelWithoutAdjustmentPoint.NodeArray[i].Comment;
                var landmark = new Landmark
                {
                    IsFromBase = true,
                    Number = i,
                    NodeId = modelWithoutAdjustmentPoint.NodeArray[i].NodeId,
                    NodeTitle = i == 0 ? rtu.Title : modelWithoutAdjustmentPoint.NodeArray[i].Title,
                    FiberId = fiber?.FiberId ?? Guid.Empty,
                    NodeComment = comment,
                    EquipmentId = equipmentId,
                    EquipmentTitle = i > 0 ? equipment?.Title : "",
                    EquipmentType = ToEquipmentType(sorLandmark.Code),
                    EventNumber = sorLandmark.RelatedEventNumber - 1,
                    LeftCableReserve = equipment?.CableReserveLeft ?? 0,
                    RightCableReserve = equipment?.CableReserveRight ?? 0,
                    GpsDistance = gpsDistance,
                    GpsSection = section,
                    IsUserInput = fiber == null ? false : fiber.UserInputedLength > 0,
                    OpticalDistance = sorData.OwtToLenKm(sorLandmark.Location),
                    OpticalSection = sorData.OwtToLenKm(sorLandmark.Location - prevOwt),
                    GpsCoors = GetPointLatLng(sorLandmark),
                };
                result.Add(landmark);
                prevOwt = sorLandmark.Location;
            }
            return result;
        }

        private PointLatLng GetPointLatLng(Optixsoft.SorExaminer.OtdrDataFormat.Structures.Landmark landmark)
        {
            var lat = SorIntCoorToDoubleInDegrees(landmark.GpsLatitude);
            var lng = SorIntCoorToDoubleInDegrees(landmark.GpsLongitude);
            return new PointLatLng(lat, lng);
        }

        private double SorIntCoorToDoubleInDegrees(int coor)
        {
            var degrees = Math.Truncate(coor / 1e6);
            var minutes = Math.Truncate(coor % 1e6 / 1e4);
            var seconds = (coor % 1e4) / 100;
            return degrees + minutes / 60 + seconds / 3600;
        }

        private static EquipmentType ToEquipmentType(LandmarkCode landmarkCode)
        {
            switch (landmarkCode)
            {
                case LandmarkCode.FiberDistributingFrame: return EquipmentType.Rtu;
                case LandmarkCode.Coupler: return EquipmentType.Closure;
                case LandmarkCode.WiringCloset: return EquipmentType.Cross;
                case LandmarkCode.Manhole: return EquipmentType.EmptyNode;
                case LandmarkCode.CableSlackLoop: return EquipmentType.CableReserve;
                case LandmarkCode.RemoteTerminal: return EquipmentType.Terminal;
                case LandmarkCode.Other: return EquipmentType.Other;
            }
            return EquipmentType.Error;
        }

    }


}