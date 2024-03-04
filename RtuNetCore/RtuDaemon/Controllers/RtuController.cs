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
            return await _commandProcessor.DoOperation(body);
        }
        catch (Exception e)
        {
            _logger.Exception(Logs.RtuService, e, "StartLongOperation");
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
            _logger.Exception(Logs.RtuService, e, "GetCurrentState");
            var error = new RtuCurrentStateDto(ReturnCode.Error) { ErrorMessage = e.Message };
            return error;
        }
    }
}