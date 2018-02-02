using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class BaseRefReAdjuster
    {
        private readonly IWcfServiceForClient _c2DWcfManager;

        public BaseRefReAdjuster(IWcfServiceForClient c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
        }

        public async Task ReAdjustBaseRefs(Trace trace)
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
            return sorBytes;
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