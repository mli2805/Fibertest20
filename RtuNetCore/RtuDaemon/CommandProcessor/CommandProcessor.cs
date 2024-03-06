using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuMngr;
using Iit.Fibertest.UtilsNetCore;
using Newtonsoft.Json;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Iit.Fibertest.RtuDaemon;

public class CommandProcessor(ILogger<CommandProcessor> logger, IWritableConfig<RtuConfig> config,
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

        switch (o)
        {
            case InitializeRtuDto dto:
                if (rtuManager.InitializationResult == null)
                    return new RequestAnswer(ReturnCode.RtuInitializationInProgress);
                Task.Factory.StartNew(() => rtuManager.InitializeRtu(dto, false));
                return new RtuInitializedDto(ReturnCode.InProgress) { Version = rtuManager.Version };
            case AssignBaseRefsDto dto:
                return rtuManager.SaveBaseRefs(dto);
            case ApplyMonitoringSettingsDto dto:
                if (rtuManager.InitializationResult == null)
                    return new RequestAnswer(ReturnCode.RtuInitializationInProgress);
                Task.Factory.StartNew(() => rtuManager.ApplyMonitoringSettings(dto));
                return new RequestAnswer(ReturnCode.InProgress);
            case AttachOtauDto dto:
                return rtuManager.AttachOtau(dto);
            case DetachOtauDto dto:
                return rtuManager.DetachOtau(dto);
            case StopMonitoringDto _:
                return await rtuManager.StopMonitoring();
            case DoClientMeasurementDto dto:
                if (rtuManager.InitializationResult == null)
                    return new ClientMeasurementStartedDto(ReturnCode.RtuInitializationInProgress);
                if (config.Value.Monitoring.IsAutoBaseMeasurementInProgress)
                    return new ClientMeasurementStartedDto(ReturnCode.RtuAutoBaseMeasurementInProgress);
                Task.Factory.StartNew(() => rtuManager.DoClientMeasurement(dto));
                return new ClientMeasurementStartedDto(ReturnCode.MeasurementClientStartedSuccessfully);
            case DoOutOfTurnPreciseMeasurementDto dto:
                if (rtuManager.InitializationResult == null)
                    return new RequestAnswer(ReturnCode.RtuInitializationInProgress);
                if (config.Value.Monitoring.IsAutoBaseMeasurementInProgress)
                    return new RequestAnswer(ReturnCode.RtuAutoBaseMeasurementInProgress);
                Task.Factory.StartNew(() => rtuManager.StartOutOfTurnMeasurement(dto));
                return new RequestAnswer(ReturnCode.InProgress);
            case InterruptMeasurementDto dto:
                return await rtuManager.InterruptMeasurement(dto);
            case FreeOtdrDto _:
                return rtuManager.FreeOtdr();
        }
        return new RequestAnswer(ReturnCode.UnknownCommand);
    }

    public async Task<RtuCurrentStateDto> GetCurrentState(string json)
    {
        var dto = JsonConvert.DeserializeObject<GetCurrentRtuStateDto>(json, JsonSerializerSettings);
        if (dto == null)
            return new RtuCurrentStateDto(ReturnCode.DeserializationError);
        if (config.Value.General.RtuId != dto.RtuId) return new RtuCurrentStateDto(ReturnCode.WrongDataCenter);

        var rtuCurrentStateDto = new RtuCurrentStateDto(ReturnCode.Ok)
        {
            LastInitializationResult = rtuManager.InitializationResult,
            CurrentStepDto = rtuManager.CurrentStep,
            MonitoringResultDtos = await GetMonitoringResults(dto.LastMeasurementTimestamp),
            ClientMeasurementResultDtos = await GetClientMeasurements(),
            BopStateChangedDtos = await GetBopEvents(),
        };

        return rtuCurrentStateDto;
    }

    private async Task<List<MonitoringResultDto>> GetMonitoringResults(DateTime lastReceived)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<MonitoringResultsRepository>();
        return await repository.GetPortionYoungerThan(lastReceived);
    }

    private async Task<List<ClientMeasurementResultDto>> GetClientMeasurements()
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ClientMeasurementsRepository>();
        var ff = await repository.GetAll();
        return ff.Select(f=>f.FromEf()).ToList();
    }

    private async Task<List<BopStateChangedDto>> GetBopEvents()
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<BopEventsRepository>();
        var ff = await repository.GetAll();
        return ff.Select(f=>f.FromEf()).ToList();
    }
}