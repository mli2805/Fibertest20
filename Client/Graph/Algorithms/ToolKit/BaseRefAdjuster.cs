using System.Linq;
using Iit.Fibertest.Dto;
using Optixsoft.SorExaminer.OtdrDataFormat;
using OpxLandmark = Optixsoft.SorExaminer.OtdrDataFormat.Structures.Landmark;

namespace Iit.Fibertest.Graph.Algorithms
{
    public enum CountMatch
    {
        LandmarksMatchNodes,
        LandmarksMatchEquipments,
        Error,
    }
    public class BaseRefAdjuster
    {
        private readonly ReadModel _readModel;

        public BaseRefAdjuster(ReadModel readModel)
        {
            _readModel = readModel;
        }

        public void InsertLandmarks(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
             var newLandmarks = new OpxLandmark[model.EquipArray.Length];

            var oldLandmarkIndex = 0;
            for (var i = 0; i < model.EquipArray.Length; i++)
            {
                if (model.EquipArray[i].Type > EquipmentType.CableReserve)
                {
                    newLandmarks[i] = sorData.LinkParameters.LandmarkBlocks[oldLandmarkIndex];
                    oldLandmarkIndex++;
                }
                else
                    newLandmarks[i] = new OpxLandmark() {Code = LandmarkCode.Manhole};
            }

            sorData.LinkParameters.LandmarkBlocks = newLandmarks;
            sorData.LinkParameters.LandmarksCount = (short)newLandmarks.Length;
        }

        public void AddNamesAndTypesForLandmarks(OtdrDataKnownBlocks sorData, Trace trace)
        {
            var nodes = _readModel.GetTraceNodes(trace).ToList();
            var equipments = _readModel.GetTraceEquipments(trace).ToList(); // without RTU
            var rtu = _readModel.Rtus.First(r => r.NodeId == nodes[0].Id);

            var landmarks = sorData.LinkParameters.LandmarkBlocks;
            landmarks[0].Comment = rtu.Title;
            for (int i = 1; i < landmarks.Length; i++)
            {
                landmarks[i].Comment = nodes[i].Title;
                if (!string.IsNullOrEmpty(equipments[i-1].Title))
                    landmarks[i].Comment = landmarks[i].Comment + $@" / {equipments[i - 1].Title}";
                landmarks[i].Code = equipments[i - 1].Type.ToLandmarkCode();
            }
        }

    }
}