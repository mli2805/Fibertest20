using Iit.Fibertest.IitOtdrLibrary;

namespace Iit.Fibertest.Graph.Algorithms
{
    public class BaseRefRepairman
    {
        private readonly WriteModel _writeModel;
        private readonly TraceModelBuilder _traceModelBuilder;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;

        public BaseRefRepairman(WriteModel writeModel,
            TraceModelBuilder traceModelBuilder, BaseRefLandmarksTool baseRefLandmarksTool)
        {
            _writeModel = writeModel;
            _traceModelBuilder = traceModelBuilder;
            _baseRefLandmarksTool = baseRefLandmarksTool;
        }

        public byte[] Modify(Trace trace , byte[] sorBytes)
        {
            var traceModel = _writeModel.GetTraceComponentsByIds(trace);
            var modelWithDistances = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(traceModel);
            var sorData = SorData.FromBytes(sorBytes);
            _baseRefLandmarksTool.SetLandmarksLocation(sorData, modelWithDistances);
            return sorData.ToBytes();
        }
      
    }
}