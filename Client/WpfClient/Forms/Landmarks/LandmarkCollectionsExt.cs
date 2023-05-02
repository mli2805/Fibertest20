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
            this List<Landmark> landmarks, List<LandmarkRow> oldLandmarkRows, 
            bool withoutEmptyNodes, GpsInputMode gpsInputMode, GpsInputMode originalGpsInputMode)
        {
            var temp = withoutEmptyNodes
                ? landmarks.Where(l => l.EquipmentType != EquipmentType.EmptyNode)
                : landmarks;

            // do not search by NodeId - trace could go through the node twice (or more)
            return new ObservableCollection<LandmarkRow>(
                temp.Select(l => new LandmarkRow()
                    .FromLandmark(l, oldLandmarkRows?
                        .First(o=>o.Number == l.Number), gpsInputMode, originalGpsInputMode)));
        }
    }
}
