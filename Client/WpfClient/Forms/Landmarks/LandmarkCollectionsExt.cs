using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class LandmarkCollectionsExt
    {
        public static ObservableCollection<LandmarkRow> LandmarksToRows(
            this List<Landmark> landmarks, List<Landmark> oldLandmarks, 
            bool withoutEmptyNodes, GpsInputMode gpsInputMode)
        {
            var temp = withoutEmptyNodes
                ? landmarks.Where(l => l.EquipmentType != EquipmentType.EmptyNode)
                : landmarks;
            return new ObservableCollection<LandmarkRow>(
                temp.Select(l => new LandmarkRow()
                    .FromLandmark(l, oldLandmarks.First(o=>o.NodeId == l.NodeId), gpsInputMode)));
        }
    }
}
