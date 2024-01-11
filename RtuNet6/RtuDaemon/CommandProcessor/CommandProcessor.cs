using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNet6;
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
        private readonly IServiceProvider _serviceProvider;

        public CommandProcessor(ILogger<CommandProcessor> logger, LongOperationsQueue longOperationsQueue,
            RtuManager rtuManager, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _longOperationsQueue = longOperationsQueue;
            _rtuManager = rtuManager;
            _serviceProvider = serviceProvider;
        }

        public Task<RequestAnswer> EnqueueLongOperation(string json)
        {
            object? o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            if (o == null)
            {
                return Task.FromResult(new RequestAnswer(ReturnCode.Error));

            }
            switch (o)
            {
                case InitializeRtuDto _:
                case ApplyMonitoringSettingsDto _:
                    var commandGuid = _longOperationsQueue.EnqueueLongOperation(json);
                    return Task.FromResult(new RequestAnswer(ReturnCode.Queued) { LongOperationGuid = commandGuid });


            }

            return Task.FromResult(new RequestAnswer(ReturnCode.Ok));
        }

        public Task<RtuCurrentStateDto> GetCurrentState()
        {
            _logger.Info(Logs.RtuService, "GetCurrentState");
            return Task.FromResult(new RtuCurrentStateDto(ReturnCode.Ok)
            {
                IsRtuInitialized = _rtuManager.IsRtuInitialized,
                IsMonitoringOn = _rtuManager.IsMonitoringOn,
                CurrentStepDto = _rtuManager.CurrentStep,
            });
        }

        public async Task<List<string>> GetMessages()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventRepository = scope.ServiceProvider.GetRequiredService<EventsRepository>();
            var ff = await eventRepository.GetPortion(10);
            return ff.Select(f => f.Json).ToList();
        }
    }
}
