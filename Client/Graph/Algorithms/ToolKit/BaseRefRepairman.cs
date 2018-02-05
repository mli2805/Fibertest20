using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;

namespace Iit.Fibertest.Graph.Algorithms.ToolKit
{
    public class BaseRefRepairman
    {
        private readonly TraceModelBuilder _traceModelBuilder;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;

        public BaseRefRepairman( 
            TraceModelBuilder traceModelBuilder, BaseRefLandmarksTool baseRefLandmarksTool)
        {
            _traceModelBuilder = traceModelBuilder;
            _baseRefLandmarksTool = baseRefLandmarksTool;
        }

        public void Amend(Trace trace, List<BaseRefDto> list)
        {
         
            foreach (var baseRefDto in list)
            {
                baseRefDto.SorBytes = Modify(trace, baseRefDto.SorBytes);
            }
            ReAssignBaseRefs(trace, list);
        }

        private byte[] Modify(Trace trace , byte[] sorBytes)
        {
            var model = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(trace);
            var sorData = SorData.FromBytes(sorBytes);
            _baseRefLandmarksTool.SetLandmarksLocation(sorData, model);
            return sorData.ToBytes();
        }

        private void ReAssignBaseRefs(Trace trace, List<BaseRefDto> list)
        {
            var dto = new AssignBaseRefsDto()
            {
                TraceId =  trace.Id,
                RtuId = trace.RtuId,
                OtauPortDto = trace.OtauPort,
                BaseRefs = list.ToList(),
            };
//            var result = await _c2DWcfManager.AssignBaseRefAsync(dto);
        }
    }
}