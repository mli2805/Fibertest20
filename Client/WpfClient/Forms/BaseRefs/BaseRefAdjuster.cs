using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;
using OpxLandmark = Optixsoft.SorExaminer.OtdrDataFormat.Structures.Landmark;
namespace Iit.Fibertest.Client
{
    public class BaseRefAdjuster
    {
        private readonly TraceModelBuilder _traceModelBuilder;

        private OtdrDataKnownBlocks _otdrDataKnownBlocks;


        public BaseRefAdjuster(TraceModelBuilder traceModelBuilder)
        {
            _traceModelBuilder = traceModelBuilder;
        }

        public void AddLandmarksForEmptyNodes(OtdrDataKnownBlocks otdrDataKnownBlocks, Trace trace)
        {
            _otdrDataKnownBlocks = otdrDataKnownBlocks;
            var originalModel = _traceModelBuilder.GetModel(trace);
            var modelWithoutAdjustmentPoint = _traceModelBuilder.ExcludeAdjustmentPoints(originalModel);

            InsertLandmarks(modelWithoutAdjustmentPoint);
            SetLandmarksLocation(modelWithoutAdjustmentPoint);
        }
        

        private void InsertLandmarks(TraceModelForBaseRef model)
        {
             var newLandmarks = new OpxLandmark[model.EquipArray.Length];

            var oldLandmarkIndex = 0;
            for (var i = 0; i < model.EquipArray.Length; i++)
            {
                if (model.EquipArray[i].Type > EquipmentType.CableReserve)
                {
                    newLandmarks[i] = _otdrDataKnownBlocks.LinkParameters.LandmarkBlocks[oldLandmarkIndex];
                    oldLandmarkIndex++;
                }
                else
                    newLandmarks[i] = new OpxLandmark() {Code = LandmarkCode.Manhole};
            }

            _otdrDataKnownBlocks.LinkParameters.LandmarkBlocks = newLandmarks;
            _otdrDataKnownBlocks.LinkParameters.LandmarksCount = (short)newLandmarks.Length;
        }

        private void SetLandmarksLocation(TraceModelForBaseRef model)
        {
            var landmarks = _otdrDataKnownBlocks.LinkParameters.LandmarkBlocks;

            for (int i = 1; i < model.EquipArray.Length; i++)
            {
                if (landmarks[i].Location != 0) continue;

                var ratio = GetRatioBaseRefToGraphAroundEmptyNode(model, i);
                landmarks[i].Location = landmarks[i - 1].Location + _otdrDataKnownBlocks.GetOwtFromMm((int)(model.Distances[i-1] * ratio));
            }
        }
        
        
        private double GetRatioBaseRefToGraphAroundEmptyNode(TraceModelForBaseRef model, int emptyNodeIndex)
        {
            var leftRealEquipmentIndex = emptyNodeIndex - 1;
            while (model.EquipArray[leftRealEquipmentIndex].Type <= EquipmentType.CableReserve) leftRealEquipmentIndex--;
            var rightRealEquipmentIndex = emptyNodeIndex + 1;
            while (model.EquipArray[rightRealEquipmentIndex].Type <= EquipmentType.CableReserve) rightRealEquipmentIndex++;

            var onGraph = GetDistanceBetweenRealEquipmentsOnGraphMm(model, leftRealEquipmentIndex, rightRealEquipmentIndex);
            var onBaseRef =
                _otdrDataKnownBlocks.GetDistanceBetweenLandmarksInMm(leftRealEquipmentIndex, rightRealEquipmentIndex);
            return ((double)onBaseRef) / onGraph;
        }

        private int GetDistanceBetweenRealEquipmentsOnGraphMm(TraceModelForBaseRef model, int leftEquipmentIndex, int rightEquipmentIndex)
        {
            if (rightEquipmentIndex - leftEquipmentIndex == 1)
                return model.Distances[leftEquipmentIndex];

            return model.Distances[leftEquipmentIndex] +
                       GetDistanceBetweenRealEquipmentsOnGraphMm(model, leftEquipmentIndex + 1, rightEquipmentIndex);
        }
       
      
       
    }
}