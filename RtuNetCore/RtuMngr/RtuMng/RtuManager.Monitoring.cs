using System.Globalization;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;

namespace Iit.Fibertest.RtuMngr;

public partial class RtuManager
{
    private bool _saveSorData;
    public async Task RunMonitoringCycle()
    {
        _saveSorData = _config.Value.Monitoring.ShouldSaveSorData;
        _logger.EmptyAndLog(Logs.RtuManager, "Run monitoring cycle.");
        _rtuManagerCts = new CancellationTokenSource();
        var tokens = new[] { RtuServiceCancellationToken, _rtuManagerCts.Token };

        var monitoringPort = await GetNextPortForMonitoring();
        if (monitoringPort == null)
        {
            _logger.Info(Logs.RtuManager, "There are no ports in queue for monitoring.");
            IsMonitoringOn = false;
            _otdrManager.DisconnectOtdr();
            _config.Update(c => c.Monitoring.IsMonitoringOnPersisted = false);
            return;
        }
        _logger.Debug(Logs.RtuManager, $"Monitoring port is {monitoringPort.CharonSerial}:{monitoringPort.OpticalPort}");

        _config.Update(c => c.Monitoring.LastMeasurementTimestamp = DateTime.Now.ToString(CultureInfo.CurrentCulture));
        _config.Update(c => c.Monitoring.IsMonitoringOnPersisted = true);
        IsMonitoringOn = true;

        while (true)
        {
            if (RtuServiceCancellationToken.IsCancellationRequested) break;
            _measurementNumber++;

            // could be the very first measurement for port, so LastMoniResult is null
            var previousUserReturnCode = monitoringPort!.LastMoniResult?.UserReturnCode ?? ReturnCode.Ok;
            var previousHardwareReturnCode = monitoringPort.LastMoniResult?.HardwareReturnCode ?? ReturnCode.Ok;

            await ProcessOnePort(tokens, monitoringPort);

            if (monitoringPort.LastMoniResult!.HardwareReturnCode == ReturnCode.MeasurementInterrupted)
            {
                // monitoringPort in memory changed
                // monitoringPort уже изменился, а измерение прервали, надо откатить
                monitoringPort.LastMoniResult.UserReturnCode = previousUserReturnCode;
                monitoringPort.LastMoniResult.HardwareReturnCode = previousHardwareReturnCode;
            }

            await PersistMonitoringPort(monitoringPort);


            if (!IsMonitoringOn)
            {
                _logger.Info(Logs.RtuManager, "IsMonitoringOn is FALSE. Leave monitoring cycle.");
                break;
            }

            monitoringPort = await GetNextPortForMonitoring();
        }

        _logger.Info(Logs.RtuManager, "RTU is turned into MANUAL mode.");
    }

    private async Task ProcessOnePort(CancellationToken[] tokens, MonitoringPort monitoringPort)
    {
        var hasFastPerformed = false;

        var isNewTrace = monitoringPort.LastTraceState == FiberState.Unknown;
        // FAST 
        if ((_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastFastSavedTimestamp > _fastSaveTimespan) ||
            (monitoringPort.LastTraceState == FiberState.Ok || isNewTrace))
        {
            monitoringPort.LastMoniResult = await DoFullMeasurement(tokens, monitoringPort, BaseRefType.Fast);
            if (!monitoringPort.LastMoniResult.IsMeasurementEndedNormally)
                return;
            hasFastPerformed = true;
        }

        if (tokens.IsCancellationRequested()) return;

        var isTraceBroken = monitoringPort.LastTraceState != FiberState.Ok;
        var isSecondMeasurementNeeded =
            isNewTrace ||
            isTraceBroken ||
            _preciseMakeTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastPreciseMadeTimestamp > _preciseMakeTimespan;

        if (isSecondMeasurementNeeded)
        {
            // PRECISE (or ADDITIONAL)
            var baseType = (isTraceBroken && monitoringPort.IsBreakdownCloserThen20Km &&
                            monitoringPort.HasAdditionalBase())
                ? BaseRefType.Additional
                : BaseRefType.Precise;

            monitoringPort.LastMoniResult = await DoFullMeasurement(tokens, monitoringPort, baseType, !hasFastPerformed);
            if (monitoringPort.LastMoniResult.UserReturnCode == ReturnCode.MeasurementEndedNormally)
                monitoringPort.IsConfirmationRequired = false;
        }

        monitoringPort.IsMonitoringModeChanged = false;
    }

    private async Task<MoniResult> DoFullMeasurement(CancellationToken[] tokens, MonitoringPort monitoringPort, 
        BaseRefType baseType, bool shouldChangePort = true, bool isOutOfTurnMeasurement = false)
    {
        _logger.EmptyAndLog(Logs.RtuManager,
            $"MEAS. {_measurementNumber}, {baseType}, port {monitoringPort.ToStringB(_mainCharon)}");

        var moniResult = await DoMeasurement(tokens, baseType, monitoringPort, shouldChangePort);
        if (moniResult.IsMeasurementEndedNormally)
        {
            if (moniResult.GetAggregatedResult() != FiberState.Ok)
                monitoringPort.IsBreakdownCloserThen20Km = moniResult.FirstBreakDistance < 20;

            var reason = BuildReason(moniResult, monitoringPort, baseType, isOutOfTurnMeasurement);
            await ApplyMoniResult(moniResult, monitoringPort, baseType, reason);
        }
        else
            await LogFailedMeasurement(moniResult, monitoringPort);

        return moniResult;
    }

    private ReasonToSendMonitoringResult BuildReason(MoniResult moniResult, MonitoringPort monitoringPort,
          BaseRefType baseType, bool isOutOfTurnMeasurement = false)
    {
        var reason = ReasonToSendMonitoringResult.None;
        if (monitoringPort.LastTraceState == FiberState.Unknown) // 740)
        {
            _logger.Info(Logs.RtuManager, "First measurement on port");
            reason |= ReasonToSendMonitoringResult.FirstMeasurementOnPort;
        }

        if (isOutOfTurnMeasurement)
        {
            _logger.Info(Logs.RtuManager, "It's out of turn precise measurement");
            reason |= ReasonToSendMonitoringResult.OutOfTurnPreciseMeasurement;
        }

        if (moniResult.GetAggregatedResult() != monitoringPort.LastTraceState)
        {
            _logger.Info(Logs.RtuManager,
                $"Trace state changed ({monitoringPort.LastTraceState} => {moniResult.GetAggregatedResult()})");
            reason |= ReasonToSendMonitoringResult.TraceStateChanged;
        }

        if (monitoringPort.IsConfirmationRequired)
        {
            _logger.Info(Logs.RtuManager, "Accident confirmation - should be saved");
            reason |= ReasonToSendMonitoringResult.OpticalAccidentConfirmation;
        }

        if (baseType == BaseRefType.Fast)
        {
            if (_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastFastSavedTimestamp > _fastSaveTimespan)
            {
                _logger.Info(Logs.RtuManager,
                    $"last fast saved - {monitoringPort.LastFastSavedTimestamp}, _fastSaveTimespan - {_fastSaveTimespan.TotalMinutes} minutes");
                _logger.Info(Logs.RtuManager, "It's time to save fast reflectogram");
                reason |= ReasonToSendMonitoringResult.TimeToRegularSave;
            }
        }
        else
        {
            if (_preciseSaveTimespan != TimeSpan.Zero &&
                DateTime.Now - monitoringPort.LastPreciseSavedTimestamp > _preciseSaveTimespan)
            {
                _logger.Info(Logs.RtuManager, "It's time to save precise reflectogram");
                reason |= ReasonToSendMonitoringResult.TimeToRegularSave;
            }
        }

        if ((monitoringPort.LastMoniResult == null && moniResult.UserReturnCode != ReturnCode.MeasurementEndedNormally) ||
            (monitoringPort.LastMoniResult != null && moniResult.UserReturnCode != monitoringPort.LastMoniResult.UserReturnCode))
        {
            var previous = monitoringPort.LastMoniResult == null
                ? "Unknown"
                : monitoringPort.LastMoniResult.UserReturnCode.ToString();
            _logger.Info(Logs.RtuManager,
                $"previous measurement code - {previous}, now - {moniResult.UserReturnCode}");
            reason |= ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged;
        }
        return reason;
    }

    private async Task ApplyMoniResult(MoniResult moniResult, MonitoringPort monitoringPort,
         BaseRefType baseType, ReasonToSendMonitoringResult reason)
    {
        monitoringPort.SetMadeTimeStamp(baseType);
        monitoringPort.LastMoniResult = moniResult;
        monitoringPort.LastTraceState = moniResult.GetAggregatedResult();

        if (reason != ReasonToSendMonitoringResult.None)
        {
            _logger.Info(Logs.RtuManager, "ApplyMoniResult going to persist monitoring result");
            await PersistMoniResultForServer(moniResult.ToEf(monitoringPort, _config.Value.General.RtuId, reason));
            monitoringPort.SetSavedTimeStamp(baseType);
        }
        await PersistMonitoringPort(monitoringPort);
    }

    private async Task LogFailedMeasurement(MoniResult moniResult, MonitoringPort monitoringPort)
    {
        if (moniResult.UserReturnCode != monitoringPort.LastMoniResult!.UserReturnCode)
        {
            _logger.Error(Logs.RtuManager,
                $"{monitoringPort.LastMoniResult.UserReturnCode} => {moniResult.UserReturnCode}");
            _logger.Error(Logs.RtuManager, "Problem with base ref occurred!");
            await PersistMoniResultForServer(moniResult.ToEf(monitoringPort, _config.Value.General.RtuId,
                ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged));
            await PersistMonitoringPort(monitoringPort);
        }
        else
        {
            _logger.Info(Logs.RtuManager, "Problem already reported.");
        }

        // other problems provoke service restart
        // and will be discovered by user only if there are many
    }

    private async Task<MoniResult> DoMeasurement(CancellationToken[] tokens, BaseRefType baseRefType, MonitoringPort monitoringPort, bool shouldChangePort = true)
    {
        if (shouldChangePort && !(await ToggleToPort(monitoringPort)))
            return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode, ReturnCode.MeasurementToggleToPortFailed);

        var baseBytes = monitoringPort.GetBaseBytes(baseRefType, _logger);
        if (baseBytes == null)
            return new MoniResult() { UserReturnCode = ReturnCode.MeasurementBaseRefNotFound, BaseRefType = baseRefType };

        _currentStep = CreateStepDto(MonitoringCurrentStep.Measure, monitoringPort, baseRefType);

        if (tokens.IsCancellationRequested()) // command to interrupt monitoring came while port toggling
            return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode, ReturnCode.MeasurementInterrupted);

        var result = _otdrManager
            .MeasureWithBase(tokens, baseBytes, _mainCharon.GetActiveChildCharon());

        switch (result)
        {
            case ReturnCode.MeasurementInterrupted:
                IsMonitoringOn = false;
                _currentStep = CreateStepDto(MonitoringCurrentStep.Interrupted);
                return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode, ReturnCode.MeasurementInterrupted);

            case ReturnCode.MeasurementFailedToSetParametersFromBase:
                // сообщить пользователю, восстановление не нужно
                return new MoniResult() { UserReturnCode = result, BaseRefType = baseRefType };

            case ReturnCode.MeasurementError:
                if (await RunMainCharonRecovery() != ReturnCode.Ok)
                    await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode,
                    result); // восстановление, без сообщения
        }

        var buffer = _otdrManager.GetLastSorDataBuffer();
        if (buffer == null)
        {
            if (await RunMainCharonRecovery() != ReturnCode.Ok)
                await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode,
                ReturnCode.MeasurementError);// восстановление, без сообщения
        }

        if (_saveSorData)
            monitoringPort.SaveSorData(baseRefType, buffer, SorType.Raw, _logger); // for investigations purpose
        monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Raw, _logger); // for investigations purpose
        _logger.Info(Logs.RtuManager, $"Measurement returned ({buffer.Length} bytes).");


        try
        {
            // sometimes GetLastSorDataBuffer returns not full sor data, so
            // just to check whether OTDR still works and measurement is reliable
            if (!_interOpWrapper.PrepareMeasurement(true))
            {
                _logger.Info(Logs.RtuManager, "Additional check after measurement failed!");
                monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Error, _logger); // save meas if error
                await RunMainCharonRecovery();
                return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode, ReturnCode.MeasurementHardwareProblem);
            }
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"Exception during PrepareMeasurement: {e.Message}");
            return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode, ReturnCode.MeasurementHardwareProblem);
        }

        var moniResult = AnalyzeAndCompare(baseRefType, monitoringPort, buffer, baseBytes);
        _config.Update(c => c.Monitoring.LastMeasurementTimestamp = DateTime.Now.ToString(CultureInfo.CurrentCulture));
        return moniResult;
    }

    private MoniResult AnalyzeAndCompare(BaseRefType baseRefType, MonitoringPort monitoringPort,
        byte[] buffer, byte[] baseBytes)
    {
        var moniResult = AnalyzeMeasurement(baseRefType, monitoringPort, buffer);
        if (moniResult.UserReturnCode == ReturnCode.MeasurementAnalysisFailed) return moniResult;

        moniResult = CompareWithBase(baseRefType, monitoringPort, moniResult.SorBytes, baseBytes);
        if (moniResult.UserReturnCode == ReturnCode.MeasurementComparisonFailed) return moniResult;

        LastSuccessfulMeasTimestamp = DateTime.Now;

        _logger.Info(Logs.RtuManager, $"Trace state is {moniResult.GetAggregatedResult()}");
        if (moniResult.Accidents != null)
            foreach (var accidentInSor in moniResult.Accidents)
                _logger.Info(Logs.RtuManager, accidentInSor.ToString() ?? string.Empty);
        return moniResult;
    }

    private MoniResult AnalyzeMeasurement(BaseRefType baseRefType, MonitoringPort monitoringPort,
        byte[] buffer)
    {
        try
        {
            _logger.Info(Logs.RtuManager, "Start auto analysis.");
            var measBytes = _otdrManager.ApplyAutoAnalysis(buffer);
            if (measBytes == null)
            {
                return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode,
                    ReturnCode.MeasurementAnalysisFailed);
            }
            monitoringPort.SaveMeasBytes(baseRefType, measBytes, SorType.Analysis, _logger);
            _logger.Info(Logs.RtuManager, $"Auto analysis applied. Now sor data has {measBytes.Length} bytes.");

            return new MoniResult() { SorBytes = measBytes };
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"Exception during measurement analysis: {e.Message}");
            return new MoniResult() { UserReturnCode = ReturnCode.MeasurementAnalysisFailed };
        }
    }

    private MoniResult CompareWithBase(BaseRefType baseRefType, MonitoringPort monitoringPort,
        byte[] measBytes, byte[] baseBytes)
    {
        try
        {
            _logger.Info(Logs.RtuManager, "Compare with base ref.");
            // base will be inserted into meas during comparison
            var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true);

            monitoringPort.SaveMeasBytes(baseRefType, measBytes, SorType.Meas, _logger); // so re-save meas after comparison
            moniResult.BaseRefType = baseRefType;
            return moniResult;
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"Exception during comparison: {e.Message}");
            return new MoniResult() { UserReturnCode = ReturnCode.MeasurementComparisonFailed };
        }
    }

    // private MonitoringResultEf ToEf(MoniResult moniResult, MonitoringPort monitoringPort,
    //     ReasonToSendMonitoringResult reason)
    // {
    //     var dto = new MonitoringResultEf()
    //     {
    //         ReturnCode = moniResult.UserReturnCode,
    //         Reason = reason,
    //         RtuId = _config.Value.General.RtuId,
    //         TimeStamp = DateTime.Now,
    //         Serial = monitoringPort.CharonSerial,
    //         IsPortOnMainCharon = monitoringPort.IsPortOnMainCharon,
    //         OpticalPort = monitoringPort.OpticalPort,
    //         TraceId = monitoringPort.TraceId,
    //         BaseRefType = moniResult.BaseRefType,
    //         TraceState = moniResult.GetAggregatedResult(),
    //         SorBytes = moniResult.SorBytes
    //     };
    //     return dto;
    // }
}