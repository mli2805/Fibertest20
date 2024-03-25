using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;

namespace Iit.Fibertest.RtuMngr;

public partial class RtuManager
{
    public async Task<RequestAnswer> ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
    {
        if (dto.IsMonitoringOn)
        {
            // user received InProgress and will start polling RTU and should wait until new initialization will be done
            InitializationResult = null;
        }

        if (IsMonitoringOn)
        {
            await BreakMonitoringCycle("Apply monitoring settings");
            _otdrManager.DisconnectOtdr();
        }

        if (!dto.IsMonitoringOn)
            await UpdateIsMonitoringOn(dto.IsMonitoringOn);
        SaveNewFrequenciesInConfig(dto.Timespans);
        var count = await CreateNewQueue(dto.Ports);
        _logger.Info(Logs.RtuManager, $"Queue merged. {count} port(s) in queue");

        if (dto.IsMonitoringOn)
            await StartMonitoring();
        return new RequestAnswer(ReturnCode.MonitoringSettingsAppliedSuccessfully);
    }

    private void SaveNewFrequenciesInConfig(MonitoringTimespansDto dto)
    {
        _config.Update(c => c.Monitoring.PreciseMakeTimespan = (int)dto.PreciseMeas.TotalSeconds);
        _preciseMakeTimespan = dto.PreciseMeas;
        _config.Update(c => c.Monitoring.PreciseSaveTimespan = (int)dto.PreciseSave.TotalSeconds);
        _preciseSaveTimespan = dto.PreciseSave;
        _config.Update(c => c.Monitoring.FastSaveTimespan = (int)dto.FastSave.TotalSeconds);
        _fastSaveTimespan = dto.FastSave;
    }

    private async Task StartMonitoring()
    {
        var rtuInitializationResult = await InitializeRtu(null, false); // will corrupt IsMonitoringOn
        if (!rtuInitializationResult.IsInitialized)
        {
            while (await RunMainCharonRecovery() != ReturnCode.Ok) { }
        }

        await UpdateIsMonitoringOn(true);
        IsMonitoringOn = true;

        _logger.EmptyAndLog(Logs.RtuManager, "RTU is turned into AUTOMATIC mode.");

        var _ = Task.Run(RunMonitoringCycle);
    }
}