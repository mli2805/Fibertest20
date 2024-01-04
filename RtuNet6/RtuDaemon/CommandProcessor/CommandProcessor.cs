using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuDaemon
{
    public class CommandProcessor
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        private readonly ILogger<CommandProcessor> _logger;
        private readonly LongOperationsQueue _longOperationsQueue;
        private readonly RtuManager _rtuManager;

        public CommandProcessor(ILogger<CommandProcessor> logger, LongOperationsQueue longOperationsQueue, RtuManager rtuManager)
        {
            _logger = logger;
            _longOperationsQueue = longOperationsQueue;
            _rtuManager = rtuManager;
        }

        public async Task<RequestAnswer> EnqueueLongOperation(string json)
        {
            object? o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            if (o == null)
            {
                return new RequestAnswer(ReturnCode.Error);

            }
            switch (o)
            {
                case InitializeRtuDto _:
                case ApplyMonitoringSettingsDto _:
                    var commandGuid = _longOperationsQueue.EnqueueLongOperation(json);
                    return new RequestAnswer(ReturnCode.Queued) { LongOperationGuid = commandGuid };


            }

            return new RequestAnswer(ReturnCode.Ok);
        }

        public Task<RtuCurrentStateDto> GetCurrentState()
        {
            return Task.FromResult(new RtuCurrentStateDto(ReturnCode.Ok)
            {
                IsRtuInitialized = _rtuManager.IsRtuInitialized,
                IsMonitoringOn = _rtuManager.IsMonitoringOn,
                CurrentStepDto = _rtuManager.CurrentStep,
            });
        }
    }
}
