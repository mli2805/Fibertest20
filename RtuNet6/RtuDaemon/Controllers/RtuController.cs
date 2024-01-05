using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNet6;
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

    [HttpPost("enqueue-long-operation")]
    public async Task<RequestAnswer> EnqueueLongOperation()
    {
        try
        {
            _logger.Info(Logs.RtuService, "RtuController EnqueueLongOperation");
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            return await _commandProcessor.EnqueueLongOperation(body);
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
            _logger.Info(Logs.RtuService, "RtuController GetCurrentState");
            return await _commandProcessor.GetCurrentState();
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, $"{e.Message}");
            return new RtuCurrentStateDto(ReturnCode.Error) { ErrorMessage = e.Message };
        }
    }

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