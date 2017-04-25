using System.Collections.Generic;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class LandmarksBaseParser
    {
        public List<Landmark> GetLandmarks(OtdrDataKnownBlocks sorData)
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
                    NodeTitle = titles.Length > 0 ? titles[0].Trim() : "",
                    EquipmentTitle = titles.Length > 1 ? titles[1].Trim() : "",
                    EquipmentType = ToEquipmentType(sorLandmark.Code),
                    EventNumber = sorLandmark.RelatedEventNumber,
                    Location = sorData.OwtToLenKm(sorLandmark.Location),
                    GpsCoors = new PointLatLng(sorLandmark.GpsLatitude, sorLandmark.GpsLongitude)
                };
                result.Add(landmark);
            }
            return result;
        }

        private static EquipmentType ToEquipmentType(LandmarkCode landmarkCode)
        {
            switch (landmarkCode)
            {
                case LandmarkCode.FiberDistributingFrame: return EquipmentType.Rtu;
                case LandmarkCode.Coupler: return EquipmentType.Closure;
                case LandmarkCode.WiringCloset: return EquipmentType.Cross;
                case LandmarkCode.Manhole: return EquipmentType.Well;
                case LandmarkCode.RemoteTerminal: return EquipmentType.Terminal;
                case LandmarkCode.Other: return EquipmentType.Other;
            }
            return EquipmentType.None;
        }

    }


}