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
        private readonly RtuManager _rtuManager;
        private readonly IServiceProvider _serviceProvider;

        public CommandProcessor(ILogger<CommandProcessor> logger, 
            RtuManager rtuManager, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rtuManager = rtuManager;
            _serviceProvider = serviceProvider;
        }

        public async Task<RequestAnswer> StartLongOperation(string json)
        {
            //using var scope = _serviceProvider.CreateScope();
            //var repo = scope.ServiceProvider.GetRequiredService<LongOperationRepository>();
            //var guid = await repo.PersistNewCommand(json);
            //return new RequestAnswer(ReturnCode.Queued) {LongOperationGuid = guid};
            await Task.Delay(0);
            return Start(json);
        }

        public Task<RtuCurrentStateDto> GetCurrentState()
        {
            return Task.FromResult(new RtuCurrentStateDto(ReturnCode.Ok)
            {
                LastInitializationResult = _rtuManager.InitializationResult,
                CurrentStepDto = _rtuManager.CurrentStep,
            });
        }

        public async Task<RtuOperationResultDto> GetLongOperationResult(Guid commandGuid)
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<LongOperationRepository>();
            return await repo.CheckResultByCommandId(commandGuid);
        }

        public async Task<List<string>> GetMessages()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventRepository = scope.ServiceProvider.GetRequiredService<EventsRepository>();
            var ff = await eventRepository.GetPortion(10);
            return ff.Select(f => f.Json).ToList();
        }

        private RequestAnswer Start(string json)
        {
            var o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            if (o == null) 
                return new RequestAnswer(ReturnCode.DeserializationError);
            _logger.Info(Logs.RtuService, $"{o.GetType().Name} request received");

            switch (o)
            {
                case InitializeRtuDto dto:
                    if (_rtuManager.InitializationResult == null)
                        return new RequestAnswer(ReturnCode.RtuIsBusy);
                    Task.Factory.StartNew(() => _rtuManager.InitializeRtu(dto, false));
                    return new RequestAnswer(ReturnCode.InProgress);
                case ApplyMonitoringSettingsDto dto:
                    if (_rtuManager.InitializationResult == null)
                        return new RequestAnswer(ReturnCode.RtuIsBusy);
                    Task.Factory.StartNew(() => _rtuManager.ApplyMonitoringSettings(dto));
                    return new RequestAnswer(ReturnCode.InProgress);

            }
            return new RequestAnswer(ReturnCode.UnknownCommand);

        }

        private async Task<RequestAnswer> Do(string json)
        {
            var o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            if (o == null)
                return new RequestAnswer(ReturnCode.DeserializationError);

            _logger.Info(Logs.RtuService, $"{o.GetType().Name} request received");

            switch (o)
            {
                case StopMonitoringDto _:
                    return await _rtuManager.StopMonitoring();
                case AttachOtauDto dto:
                    return await _rtuManager.AttachOtau(dto);
            }

            return new RequestAnswer(ReturnCode.UnknownCommand);
        }
    }
}
