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

    [HttpPost("current-state")]
    public async Task<RtuCurrentStateDto> GetCurrentState()
    {
        try
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            return await _commandProcessor.GetCurrentState(body);
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, $"{e.Message}");
            var error = new RtuCurrentStateDto(ReturnCode.Error) { ErrorMessage = e.Message };
            return error;
        }
    }

   
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