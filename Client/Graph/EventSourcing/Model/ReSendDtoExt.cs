using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class ReSendDtoExt
    {
        public static IEnumerable<ReSendBaseRefsDto> CreateReSendDtos(this Model model, Rtu rtu, string currentUserConnectionId)
        {
            foreach (var trace in model.Traces
                         .Where(t => t.RtuId == rtu.Id && t.IsAttached && t.HasAnyBaseRef))
            {
                var dto = new ReSendBaseRefsDto()
                {
                    ConnectionId = currentUserConnectionId,
                    RtuId = rtu.Id,
                    RtuMaker = rtu.RtuMaker,
                    OtdrId = rtu.OtdrId,
                    TraceId = trace.TraceId,
                    OtauPortDto = trace.OtauPort,
                    BaseRefDtos = new List<BaseRefDto>(),
                };
                foreach (var baseRef in model.BaseRefs.Where(b => b.TraceId == trace.TraceId))
                {
                    dto.BaseRefDtos.Add(new BaseRefDto()
                    {
                        SorFileId = baseRef.SorFileId,

                        Id = baseRef.TraceId,
                        BaseRefType = baseRef.BaseRefType,
                        Duration = baseRef.Duration,
                        SaveTimestamp = baseRef.SaveTimestamp,
                        UserName = baseRef.UserName,
                    });
                }
                yield return dto;
            }
        }
    }
}