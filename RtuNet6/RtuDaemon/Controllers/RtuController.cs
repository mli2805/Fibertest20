using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNet6;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuDaemon;

[ApiController]
[Route("[controller]")]
public class RtuController : ControllerBase
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private readonly ILogger<RtuController> _logger;
    private readonly CommandProcessor _commandProcessor;

    public RtuController(ILogger<RtuController> logger, CommandProcessor commandProcessor)
    {
        _logger = logger;
        _commandProcessor = commandProcessor;
    }

    [HttpPost("start-long-operation")]
    public async Task<RequestAnswer> StartLongOperation()
    {
        try
        {
            _logger.Info(Logs.RtuService, "RtuController StartLongOperation");
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            return await _commandProcessor.StartLongOperation(body);
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, $"{e.Message}");
            return new RequestAnswer(ReturnCode.Error) { ErrorMessage = e.Message };
        }
    }

    [HttpGet("long-operation-result/{id}")]
    public async Task<RtuOperationResultDto> GetLongOperationResult(string id)
    {
        try
        {
            _logger.Info(Logs.RtuService, "RtuController GetLongOperationResult");
            var commandGuid = Guid.Parse(id);
            return await _commandProcessor.GetLongOperationResult(commandGuid);
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuService, $"{e.Message}");
            return new RtuOperationResultDto(ReturnCode.Error) { ErrorMessage = e.Message };
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