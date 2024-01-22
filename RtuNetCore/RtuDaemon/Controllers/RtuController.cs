using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Iit.Fibertest.RtuDaemon;

[ApiController]
[Route("[controller]")]
public class RtuController : ControllerBase
{
    private readonly ILogger<RtuController> _logger;
    private readonly CommandProcessor _commandProcessor;

    public RtuController(ILogger<RtuController> logger, CommandProcessor commandProcessor)
    {
        _logger = logger;
        _commandProcessor = commandProcessor;
    }

    [HttpPost("do-operation")]
    public async Task<RequestAnswer> StartLongOperation()
    {
        try
        {
            _logger.Info(Logs.RtuService, "RtuController HttpPost do-operation endpoint");
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            return _commandProcessor.DoOperation(body);
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, $"{e.Message}");
            return new RequestAnswer(ReturnCode.Error) { ErrorMessage = e.Message };
        }
    }

    [HttpGet("current-state")]
    public async Task<RtuCurrentStateDto> GetCurrentState()
    {
        try
        {
            return await _commandProcessor.GetCurrentState();
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, $"{e.Message}");
            var error = new RtuCurrentStateDto(ReturnCode.Error) { ErrorMessage = e.Message };
            return error;
        }
    }

   
    // // Full dto with base refs (sorBytes) is serialized into json on server and de-serialized here
    // [HttpPost("assign-base-refs-json")]
    // public async Task<BaseRefAssignedDto> AssignBaseRefsJson()
    // {
    //     _logger.Info(Logs.RtuService, "RtuController AssignBaseRefsJson started");
    //     try
    //     {
    //         string body;
    //         using (var reader = new StreamReader(Request.Body))
    //         {
    //             body = await reader.ReadToEndAsync();
    //         }
    //         var dto = JsonConvert.DeserializeObject<AssignBaseRefsDto>(body, JsonSerializerSettings);
    //         if (dto == null)
    //         {
    //             _logger.Info(Logs.RtuService, "Failed deserialize base refs dto");
    //             return new BaseRefAssignedDto(ReturnCode.DeserializationError);
    //         }
    //
    //         return _commandProcessor.SaveBaseRefs(dto);
    //     }
    //     catch (Exception e)
    //     {
    //         _logger.Error(Logs.RtuService, $"{e.Message}");
    //         return new BaseRefAssignedDto(ReturnCode.BaseRefAssignmentFailed) { ErrorMessage = e.Message };
    //     }
    // }


    // MonitoringResults, BopStateChanges, etc
    [HttpGet("messages")]
    public async Task<List<string>?> GetMessages()
    {
        try
        {
            _logger.Info(Logs.RtuService, "RtuController GetMessages");
            return await _commandProcessor.GetMessages();
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, $"{e.Message}");
            return null;
        }
    }

}