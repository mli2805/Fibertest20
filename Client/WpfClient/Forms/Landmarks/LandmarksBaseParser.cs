using System;
using System.Collections.Generic;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class LandmarksBaseParser
    {
        public List<Landmark> GetLandmarks(OtdrDataKnownBlocks sorData, List<Guid> nodesWithoutPoint)
        {
            var result =  new List<Landmark>();
            var linkParameters = sorData.LinkParameters;
            for (int i = 0; i < linkParameters.LandmarksCount; i++)
            {
                var sorLandmark = linkParameters.LandmarkBlocks[i];
                var titles = sorLandmark.Comment.Split('/');
                var landmark = new Landmark
                {
                    Number = i,
                    NodeId = nodesWithoutPoint[i],
                    NodeTitle = titles.Length > 0 ? titles[0].Trim() : "",
                    EquipmentTitle = titles.Length > 1 ? titles[1].Trim() : "",
                    EquipmentType = ToEquipmentType(sorLandmark.Code),
                    EventNumber = sorLandmark.RelatedEventNumber - 1,
                    Distance = sorData.OwtToLenKm(sorLandmark.Location),
                    GpsCoors = GetPointLatLng(sorLandmark),
                };
                result.Add(landmark);
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