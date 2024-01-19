using Iit.Fibertest.Dto;
using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.RtuMngr;

public class LongOperationRepository
{
    private readonly RtuContext _rtuContext;

    public LongOperationRepository(RtuContext rtuContext)
    {
        _rtuContext = rtuContext;
    }

    public async Task<Guid> PersistNewCommand(string json)
    {
        var ef = new LongOperationEf(json);
        await _rtuContext.LongOperations.AddAsync(ef);
        await _rtuContext.SaveChangesAsync();
        return ef.CommandGuid;
    }

    public async Task<LongOperationEf?> ExtractForExecution()
    {
        var dto = await _rtuContext.LongOperations.FirstOrDefaultAsync();
        if (dto != null)
        {
            _rtuContext.LongOperations.Remove(dto);
            await _rtuContext.SaveChangesAsync();
        }
        return dto;
    }

    public async Task<RtuOperationResultDto> CheckResultByCommandId(Guid commandGuid)
    {
        var dto = await _rtuContext.LongOperations.FirstOrDefaultAsync(l => l.CommandGuid == commandGuid);
        if (dto == null) return new RtuOperationResultDto(ReturnCode.NotFound);
        if (!dto.IsReady) return new RtuOperationResultDto(ReturnCode.InProgress);

        _rtuContext.LongOperations.Remove(dto);
        await _rtuContext.SaveChangesAsync();
        return new RtuOperationResultDto(ReturnCode.Ok) { ResultJson = dto.ResultJson };
    }
}