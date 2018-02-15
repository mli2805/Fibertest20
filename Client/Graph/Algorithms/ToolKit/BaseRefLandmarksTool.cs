using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Graph.Algorithms
{
    public class BaseRefLandmarksTool
    {
        public void SetLandmarksLocation(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var landmarks = sorData.LinkParameters.LandmarkBlocks;

            for (int i = 1; i < model.EquipArray.Length; i++)
            {
                if (landmarks[i].RelatedEventNumber != 0) continue; // landmark is "tied" with keyEvent and we can't move it

                var ratio = GetRatioBaseRefToGraphAroundEmptyNode(sorData, model, i);
                landmarks[i].Location = landmarks[i - 1].Location + sorData.GetOwtFromMm((int)(model.DistancesMm[i - 1] * ratio));
            }
        }


        private double GetRatioBaseRefToGraphAroundEmptyNode(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model, int emptyNodeIndex)
        {
            var leftRealEquipmentIndex = emptyNodeIndex - 1;
            while (model.EquipArray[leftRealEquipmentIndex].Type <= EquipmentType.CableReserve) leftRealEquipmentIndex--;
            var rightRealEquipmentIndex = emptyNodeIndex + 1;
            while (model.EquipArray[rightRealEquipmentIndex].Type <= EquipmentType.CableReserve) rightRealEquipmentIndex++;

            var onGraph = GetDistanceBetweenRealEquipmentsOnGraphMm(model, leftRealEquipmentIndex, rightRealEquipmentIndex);
            var onBaseRef =
                sorData.GetDistanceBetweenLandmarksInMm(leftRealEquipmentIndex, rightRealEquipmentIndex);
            return ((double)onBaseRef) / onGraph;
        }

        private int GetDistanceBetweenRealEquipmentsOnGraphMm(TraceModelForBaseRef model, int leftEquipmentIndex, int rightEquipmentIndex)
        {
            if (rightEquipmentIndex - leftEquipmentIndex == 1)
                return model.DistancesMm[leftEquipmentIndex];

            return model.DistancesMm[leftEquipmentIndex] +
                   GetDistanceBetweenRealEquipmentsOnGraphMm(model, leftEquipmentIndex + 1, rightEquipmentIndex);
        }

    }
}