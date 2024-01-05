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
        private readonly IServiceProvider _serviceProvider;
        private readonly EventsRepository _eventsRepository;

        public CommandProcessor(ILogger<CommandProcessor> logger, LongOperationsQueue longOperationsQueue,
            RtuManager rtuManager, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _longOperationsQueue = longOperationsQueue;
            _rtuManager = rtuManager;
            _serviceProvider = serviceProvider;
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

        public async Task<List<string>> GetMessages()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventRepository = scope.ServiceProvider.GetRequiredService<EventsRepository>();
            var ff = await eventRepository.GetPortion(10);
            return ff.Select(f => f.Json).ToList();
        }
    }
}
