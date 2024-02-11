﻿using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNetCore;
using Newtonsoft.Json;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Iit.Fibertest.RtuDaemon
{
    public class CommandProcessor(ILogger<CommandProcessor> logger,
        RtuManager rtuManager, IServiceProvider serviceProvider)
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        public async Task<RequestAnswer> DoOperation(string json)
        {
            var o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            if (o == null)
                return new RequestAnswer(ReturnCode.DeserializationError);
            logger.Info(Logs.RtuService, $"{o.GetType().Name} received");
            logger.TimestampWithoutMessage(Logs.RtuManager);

            switch (o)
            {
                case InitializeRtuDto dto:
                    if (rtuManager.InitializationResult == null)
                        return new RequestAnswer(ReturnCode.RtuIsBusy);
                    Task.Factory.StartNew(() => rtuManager.InitializeRtu(dto, false));
                    return new RtuInitializedDto(ReturnCode.InProgress) { Version = rtuManager.Version };
                case AssignBaseRefsDto dto:
                    return rtuManager.SaveBaseRefs(dto);
                case ApplyMonitoringSettingsDto dto:
                    if (rtuManager.InitializationResult == null)
                        return new RequestAnswer(ReturnCode.RtuIsBusy);
                    Task.Factory.StartNew(() => rtuManager.ApplyMonitoringSettings(dto));
                    return new RequestAnswer(ReturnCode.InProgress);
                case StopMonitoringDto _:
                    return await rtuManager.StopMonitoring();
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
                LastInitializationResult = rtuManager.InitializationResult,
                CurrentStepDto = rtuManager.CurrentStep,
                MonitoringResultDtos = await GetMonitoringResults(dto.LastMeasurementTimestamp)
            };

            return rtuCurrentStateDto;
        }

        private async Task<List<MonitoringResultDto>> GetMonitoringResults(DateTime lastReceived)
        {
            using var scope = serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<MonitoringResultsRepository>();
            return await repository.GetPortionYoungerThan(lastReceived);
        }

        public async Task<List<string>> GetMessages()
        {
            using var scope = serviceProvider.CreateScope();
            var eventRepository = scope.ServiceProvider.GetRequiredService<EventsRepository>();
            var ff = await eventRepository.GetPortion(10);
            return ff.Select(f => f.Json).ToList();
        }

     
    }
}
