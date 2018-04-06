using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.Graph.Algorithms
{
    public class BaseRefLandmarksTool
    {
        private readonly Model _model;

        public BaseRefLandmarksTool(Model model)
        {
            _model = model;
        }

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

        public void InsertLandmarks(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var newLandmarks = new Landmark[model.EquipArray.Length];

            var oldLandmarkIndex = 0;
            for (var i = 0; i < model.EquipArray.Length; i++)
            {
                if (model.EquipArray[i].Type > EquipmentType.CableReserve)
                {
                    newLandmarks[i] = sorData.LinkParameters.LandmarkBlocks[oldLandmarkIndex];
                    oldLandmarkIndex++;
                }
                else
                    newLandmarks[i] = new Landmark() { Code = LandmarkCode.Manhole };
            }

            sorData.LinkParameters.LandmarkBlocks = newLandmarks;
            sorData.LinkParameters.LandmarksCount = (short)newLandmarks.Length;
        }

        public void AddNamesAndTypesForLandmarks(OtdrDataKnownBlocks sorData, Trace trace)
        {
            var nodes = _model.GetTraceNodes(trace).ToList();
            var equipments = _model.GetTraceEquipments(trace).ToList(); // without RTU
            var rtu = _model.Rtus.First(r => r.NodeId == nodes[0].NodeId);

            var landmarks = sorData.LinkParameters.LandmarkBlocks;
            landmarks[0].Comment = rtu.Title;
            for (int i = 1; i < landmarks.Length; i++)
            {
                var landmarkTitle = nodes[i].Title;
                if (!string.IsNullOrEmpty(equipments[i - 1].Title))
                    landmarkTitle = landmarkTitle + $@" / {equipments[i - 1].Title}";

                landmarks[i].Comment = landmarkTitle; // utf8, TODO reflect.exe should understand this
                landmarks[i].Code = equipments[i - 1].Type.ToLandmarkCode();

                landmarks[i].GpsLatitude = GpsCalculator.GpsInSorFormat(nodes[i].Position.Lat);
                landmarks[i].GpsLongitude = GpsCalculator.GpsInSorFormat(nodes[i].Position.Lng);
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