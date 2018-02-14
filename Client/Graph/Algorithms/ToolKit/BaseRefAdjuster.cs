using System.Linq;
using Optixsoft.SorExaminer.OtdrDataFormat;
using OpxLandmark = Optixsoft.SorExaminer.OtdrDataFormat.Structures.Landmark;
namespace Iit.Fibertest.Graph.Algorithms.ToolKit
{
    public class BaseRefAdjuster
    {
        private readonly ReadModel _readModel;
        private readonly TraceModelBuilder _traceModelBuilder;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;

        public BaseRefAdjuster(ReadModel readModel, TraceModelBuilder traceModelBuilder, BaseRefLandmarksTool baseRefLandmarksTool)
        {
            _readModel = readModel;
            _traceModelBuilder = traceModelBuilder;
            _baseRefLandmarksTool = baseRefLandmarksTool;
        }

        public void AddLandmarksForEmptyNodes(OtdrDataKnownBlocks otdrDataKnownBlocks, Trace trace)
        {
            var traceModel = _readModel.GetTraceComponentsByIds(trace);
            var modelWithoutAdjustmentPoint = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(traceModel);

            InsertLandmarks(otdrDataKnownBlocks, modelWithoutAdjustmentPoint);
            _baseRefLandmarksTool.SetLandmarksLocation(otdrDataKnownBlocks, modelWithoutAdjustmentPoint);
        }

        private void InsertLandmarks(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
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

        public void AddNamesForLandmarks(OtdrDataKnownBlocks sorData, Trace trace)
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
                    landmarks[i].Comment = landmarks[i].Comment + $@"{equipments[i-1].Title}";
            }
        }

    }
}