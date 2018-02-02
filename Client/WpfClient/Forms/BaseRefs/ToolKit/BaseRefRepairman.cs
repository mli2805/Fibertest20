using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class BaseRefRepairman
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly TraceModelBuilder _traceModelBuilder;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;

        public BaseRefRepairman(IWcfServiceForClient c2DWcfManager, 
            TraceModelBuilder traceModelBuilder, BaseRefLandmarksTool baseRefLandmarksTool)
        {
            _c2DWcfManager = c2DWcfManager;
            _traceModelBuilder = traceModelBuilder;
            _baseRefLandmarksTool = baseRefLandmarksTool;
        }

        public async Task Amend(Trace trace)
        {
            var list = await _c2DWcfManager.GetTraceBaseRefsAsync(trace.Id);
            if (list == null)
                return;
            foreach (var baseRefDto in list)
            {
                baseRefDto.SorBytes = Modify(trace, baseRefDto.SorBytes);
            }
            await ReAssignBaseRefs(trace, list);
        }

        private byte[] Modify(Trace trace , byte[] sorBytes)
        {
            var model = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(trace);
            var sorData = SorData.FromBytes(sorBytes);
            _baseRefLandmarksTool.SetLandmarksLocation(sorData, model);
            return sorData.ToBytes();
        }

        private async Task ReAssignBaseRefs(Trace trace, List<BaseRefDto> list)
        {
            var dto = new AssignBaseRefsDto()
            {
                TraceId =  trace.Id,
                RtuId = trace.RtuId,
                OtauPortDto = trace.OtauPort,
                BaseRefs = list.ToList(),
            };
            var result = await _c2DWcfManager.AssignBaseRefAsync(dto);
        }
    }
}