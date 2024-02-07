using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNetCore;
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

        public RequestAnswer DoOperation(string json)
        {
            var o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            if (o == null)
                return new RequestAnswer(ReturnCode.DeserializationError);
            _logger.Info(Logs.RtuService, $"{o.GetType().Name} received");
            _logger.TimestampWithoutMessage(Logs.RtuManager);

            switch (o)
            {
                case InitializeRtuDto dto:
                    if (_rtuManager.InitializationResult == null)
                        return new RequestAnswer(ReturnCode.RtuIsBusy);
                    Task.Factory.StartNew(() => _rtuManager.InitializeRtu(dto, false));
                    return new RtuInitializedDto(ReturnCode.InProgress) { Version = _rtuManager.Version };
                case AssignBaseRefsDto dto:
                    return _rtuManager.SaveBaseRefs(dto);
                case ApplyMonitoringSettingsDto dto:
                    if (_rtuManager.InitializationResult == null)
                        return new RequestAnswer(ReturnCode.RtuIsBusy);
                    Task.Factory.StartNew(() => _rtuManager.ApplyMonitoringSettings(dto));
                    return new RequestAnswer(ReturnCode.InProgress);
            }
            return new RequestAnswer(ReturnCode.UnknownCommand);
        }

      

        public async Task<RtuCurrentStateDto> GetCurrentState(string json)
        {
            var dto = JsonConvert.DeserializeObject<GetCurrentRtuStateDto>(json, JsonSerializerSettings);
            if (dto == null)
                return new RtuCurrentStateDto(ReturnCode.DeserializationError);

            var rtuCurrentStateDto = new RtuCurrentStateDto(ReturnCode.Ok)
            {
                LastInitializationResult = _rtuManager.InitializationResult,
                CurrentStepDto = _rtuManager.CurrentStep,
                MonitoringResultDtos = await GetMonitoringResults(dto.LastMeasurementTimestamp)
            };

            return rtuCurrentStateDto;
        }

        private async Task<List<MonitoringResultDto>> GetMonitoringResults(DateTime lastReceived)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<MonitoringResultsRepository>();
            return await repository.GetPortionYoungerThan(lastReceived);
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
