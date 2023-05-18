using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class LandmarkExt
    {
        public static bool NodePropertiesChanged(this Landmark landmark, Landmark other)
        {
            return landmark.NodeTitle != other.NodeTitle
                || landmark.NodeComment != other.NodeComment
                || !landmark.AreCoordinatesTheSame(other);
        }

        public static bool EquipmentPropertiesChanged(this Landmark landmark, Landmark other)
        {
            return landmark.EquipmentTitle != other.EquipmentTitle
                   || landmark.EquipmentType != other.EquipmentType
                   || landmark.LeftCableReserve != other.LeftCableReserve
                   || landmark.RightCableReserve != other.RightCableReserve;
        }
    }
}
