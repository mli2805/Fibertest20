using System.Linq;
using Iit.Fibertest.Dto;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;
using System.Collections.Generic;
using System;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        public async Task<RequestAnswer> AttachTraceAndSendBaseRefs(AttachTraceDto dto)
        {
            var trace = _writeModel.Traces.First(t => t.TraceId == dto.TraceId);
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null)
                return new RequestAnswer() { ReturnCode = ReturnCode.NoSuchRtu };
            if (!TryToGetClientAndOccupyRtu(dto.ConnectionId, rtu.Id, RtuOccupation.AttachTrace,
                    out RequestAnswer response))
                return response;

            try
            {
                var dto1 = await CreateAssignBaseRefsDto(dto, rtu, trace);
                BaseRefAssignedDto transferResult = dto1.BaseRefs.Any() ? await TransmitBaseRefs(dto1) : null;

                if (transferResult != null && transferResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return new RequestAnswer(transferResult.ReturnCode) { ErrorMessage = transferResult.ErrorMessage };

                var cmd = new AttachTrace() { TraceId = dto.TraceId, OtauPortDto = dto.OtauPortDto };
                var res = await _eventStoreService.SendCommand(cmd, dto.Username, dto.ClientIp);

                if (!string.IsNullOrEmpty(res))
                    return new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = res };

                await NotifyWebClientTraceAttached(dto.TraceId);

                if (transferResult == null || dto.RtuMaker == RtuMaker.IIT)
                    return new RequestAnswer() { ReturnCode = ReturnCode.Ok };

                // Veex and there are base refs so veexTests table should be updated
                return await UpdateVeexTestList(transferResult, dto.Username, dto.ClientIp);
            }
            finally
            {
                _rtuOccupations.TrySetOccupation(rtu.Id, RtuOccupation.None, response.UserName, out RtuOccupationState _);
            }
        }

        private async Task<AssignBaseRefsDto> CreateAssignBaseRefsDto(AttachTraceDto cmd, Rtu rtu, Trace trace)
        {
            var dto = new AssignBaseRefsDto()
            {
                RtuId = trace.RtuId,
                RtuMaker = rtu.RtuMaker,
                OtdrId = rtu.OtdrId,
                TraceId = cmd.TraceId,
                OtauPortDto = cmd.OtauPortDto,
                MainOtauPortDto = cmd.MainOtauPortDto,
                BaseRefs = new List<BaseRefDto>(),
            };

            foreach (var baseRef in _writeModel.BaseRefs.Where(b => b.TraceId == trace.TraceId))
            {
                dto.BaseRefs.Add(new BaseRefDto()
                {
                    SorFileId = baseRef.SorFileId,
                    Id = baseRef.TraceId,
                    BaseRefType = baseRef.BaseRefType,
                    Duration = baseRef.Duration,
                    SaveTimestamp = baseRef.SaveTimestamp,
                    UserName = baseRef.UserName,

                    SorBytes = await _sorFileRepository.GetSorBytesAsync(baseRef.SorFileId),
                });
            }

            return dto;
        }

        private async Task NotifyWebClientTraceAttached(Guid traceId)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace != null)
            {
                var meas = _writeModel.Measurements.LastOrDefault(m => m.TraceId == traceId);
                var signal = new TraceTachDto()
                {
                    TraceId = traceId,
                    Attach = true,
                    TraceState = trace.State,
                    SorFileId = meas?.SorFileId ?? -1
                };

                await _ftSignalRClient.NotifyAll("TraceTach", signal.ToCamelCaseJson());
            }
        }

    }
}
