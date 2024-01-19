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

        
            var monitoringPort = await GetNextPortForMonitoring();
        if (monitoringPort == null)
        {
            _logger.Info(Logs.RtuManager, "There are no ports in queue for monitoring.");
            IsMonitoringOn = false;
            _config.Update(c => c.Monitoring.IsMonitoringOnPersisted = false);
            return;
        }
        _config.Update(c => c.Monitoring.LastMeasurementTimestamp = DateTime.Now.ToString(CultureInfo.CurrentCulture));
        _config.Update(c => c.Monitoring.IsMonitoringOnPersisted = true);
        IsMonitoringOn = true;

        while (true)
        {
            _measurementNumber++;
          
            var previousUserReturnCode = monitoringPort!.LastMoniResult!.UserReturnCode;
            var previousHardwareReturnCode = monitoringPort.LastMoniResult!.HardwareReturnCode;
            await ProcessOnePort(monitoringPort);

            if (monitoringPort.LastMoniResult!.HardwareReturnCode != ReturnCode.MeasurementInterrupted)
            {
                //var unused = _monitoringQueue.Dequeue();
                //_monitoringQueue.Enqueue(monitoringPort);
                //await _monitoringQueue.Save();
                await UpdateMonitoringPort(monitoringPort);
            }
            else
            {
                // monitoringPort in memory changed
                // monitoringPort уже изменился, а измерение прервали, надо откатить
                monitoringPort.LastMoniResult.UserReturnCode = previousUserReturnCode;
                monitoringPort.LastMoniResult.HardwareReturnCode = previousHardwareReturnCode;
                await UpdateMonitoringPort(monitoringPort);
            }

            if (!IsMonitoringOn)
            {
                _logger.Debug(Logs.RtuManager, "IsMonitoringOn is FALSE. Leave monitoring cycle.");
                break;
            }

            //var monitoringPort = _monitoringQueue.Peek();
            monitoringPort = await GetNextPortForMonitoring();
        }

        _logger.Info(Logs.RtuManager, "Monitoring stopped.");

        if (!_wasMonitoringOn)
        {
            _config.Update(c => c.Monitoring.IsMonitoringOnPersisted = false);
            _otdrManager.DisconnectOtdr();
            IsMonitoringOn = false; _logger.Info(Logs.RtuManager, "RTU is turned into MANUAL mode.");
        }
    }

    private async Task ProcessOnePort(MonitoringPort monitoringPort)
    {
        var hasFastPerformed = false;

        var isNewTrace = monitoringPort.LastTraceState == FiberState.Unknown;
        // FAST 
        if ((_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastFastSavedTimestamp > _fastSaveTimespan) ||
            (monitoringPort.LastTraceState == FiberState.Ok || isNewTrace))
        {
            monitoringPort.LastMoniResult = await DoFastMeasurement(monitoringPort);
            if (monitoringPort.LastMoniResult.IsMeasurementEndedNormally)
                return;
            hasFastPerformed = true;
        }

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

            monitoringPort.LastMoniResult = await DoSecondMeasurement(monitoringPort, hasFastPerformed, baseType);
            if (monitoringPort.LastMoniResult.UserReturnCode == ReturnCode.MeasurementEndedNormally)
                monitoringPort.IsConfirmationRequired = false;
        }

        monitoringPort.IsMonitoringModeChanged = false;
    }

    private async Task<MoniResult> DoFastMeasurement(MonitoringPort monitoringPort)
    {
        _logger.EmptyAndLog(Logs.RtuManager,
            $"MEAS. {_measurementNumber}, Fast, port {monitoringPort.ToStringB(_mainCharon)}");

        var moniResult = await DoMeasurement(BaseRefType.Fast, monitoringPort);
        var reason = ReasonToSendMonitoringResult.None;

        if (moniResult.IsMeasurementEndedNormally)
        {
            if (moniResult.GetAggregatedResult() != FiberState.Ok)
                monitoringPort.IsBreakdownCloserThen20Km = moniResult.FirstBreakDistance < 20;

            if (monitoringPort.LastTraceState == FiberState.Unknown) // 740)
            {
                _logger.Info(Logs.RtuManager, "First measurement on port");
                reason |= ReasonToSendMonitoringResult.FirstMeasurementOnPort;
            }

            if (moniResult.GetAggregatedResult() != monitoringPort.LastTraceState)
            {
                _logger.Info(Logs.RtuManager,
                    $"Trace state has changed ({monitoringPort.LastTraceState} => {moniResult.GetAggregatedResult()})");
                reason |= ReasonToSendMonitoringResult.TraceStateChanged;
                monitoringPort.IsConfirmationRequired = true;
            }

            if (_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastFastSavedTimestamp > _fastSaveTimespan)
            {
                _logger.Info(Logs.RtuManager,
                    $"last fast saved - {monitoringPort.LastFastSavedTimestamp}, _fastSaveTimespan - {_fastSaveTimespan.TotalMinutes} minutes");
                _logger.Info(Logs.RtuManager, "It's time to save fast reflectogram");
                reason |= ReasonToSendMonitoringResult.TimeToRegularSave;
            }

            if (moniResult.UserReturnCode != monitoringPort.LastMoniResult!.UserReturnCode)
            {
                _logger.Info(Logs.RtuManager,
                    $"previous measurement code - {monitoringPort.LastMoniResult.UserReturnCode}, now - {moniResult.UserReturnCode}");
                _logger.Info(Logs.RtuManager, "Problem with base ref solved");
                reason |= ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged;
            }

            monitoringPort.LastFastMadeTimestamp = DateTime.Now;
            monitoringPort.LastMoniResult = moniResult;
            monitoringPort.LastTraceState = moniResult.GetAggregatedResult();
            //await _monitoringQueue.Save();
            await UpdateMonitoringPort(monitoringPort);

            if (reason != ReasonToSendMonitoringResult.None)
            {
                await SaveMoniResult(CreateEf(moniResult, monitoringPort, reason));
                monitoringPort.LastFastSavedTimestamp = DateTime.Now;
                // await _monitoringQueue.Save();
                await UpdateMonitoringPort(monitoringPort);
            }
        }
        else
            await LogFailedMeasurement(moniResult, monitoringPort);

        return moniResult;
    }

    private async Task LogFailedMeasurement(MoniResult moniResult, MonitoringPort monitoringPort)
    {
        if (moniResult.UserReturnCode != monitoringPort.LastMoniResult!.UserReturnCode)
        {
            _logger.Error(Logs.RtuManager,
                $"{monitoringPort.LastMoniResult.UserReturnCode} => {moniResult.UserReturnCode}");
            _logger.Error(Logs.RtuManager, "Problem with base ref occurred!");
            await SaveMoniResult(CreateEf(moniResult, monitoringPort,
                ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged));
            //await _monitoringQueue.Save();
                await UpdateMonitoringPort(monitoringPort);
        }
        else
        {
            _logger.Info(Logs.RtuManager, "Problem already reported.");
        }

        // other problems provoke service restart
        // and will be discovered by user only if there are many
    }

    private async Task<MoniResult> DoSecondMeasurement(MonitoringPort monitoringPort, bool hasFastPerformed,
            BaseRefType baseType, bool isOutOfTurnMeasurement = false)
    {
        _logger.EmptyAndLog(Logs.RtuManager,
            $"MEAS. {_measurementNumber}, {baseType}, port {monitoringPort.ToStringB(_mainCharon)}");

        var moniResult = await DoMeasurement(baseType, monitoringPort, !hasFastPerformed);

        if (moniResult.IsMeasurementEndedNormally)
        {
            var reason = ReasonToSendMonitoringResult.None;
            if (isOutOfTurnMeasurement)
            {
                _logger.Info(Logs.RtuManager, "It's out of turn precise measurement");
                reason |= ReasonToSendMonitoringResult.OutOfTurnPreciseMeasurement;
            }

            if (moniResult.GetAggregatedResult() != monitoringPort.LastTraceState)
            {
                _logger.Info(Logs.RtuManager,
                    $"Trace state has changed ({monitoringPort.LastTraceState} => {moniResult.GetAggregatedResult()})");
                reason |= ReasonToSendMonitoringResult.TraceStateChanged;
            }

            if (monitoringPort.IsConfirmationRequired)
            {
                _logger.Info(Logs.RtuManager, "Accident confirmation - should be saved");
                reason |= ReasonToSendMonitoringResult.OpticalAccidentConfirmation;
            }

            if (_preciseSaveTimespan != TimeSpan.Zero &&
                DateTime.Now - monitoringPort.LastPreciseSavedTimestamp > _preciseSaveTimespan)
            {
                _logger.Info(Logs.RtuManager, "It's time to save precise reflectogram");
                reason |= ReasonToSendMonitoringResult.TimeToRegularSave;
            }

            if (moniResult.UserReturnCode != monitoringPort.LastMoniResult!.UserReturnCode)
            {
                _logger.Info(Logs.RtuManager,
                    $"previous measurement code - {monitoringPort.LastMoniResult.UserReturnCode}, now - {moniResult.UserReturnCode}");
                _logger.Info(Logs.RtuManager, "Problem with base ref solved");
                reason |= ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged;
            }

            monitoringPort.LastPreciseMadeTimestamp = DateTime.Now;
            monitoringPort.LastMoniResult = moniResult;
            monitoringPort.LastTraceState = moniResult.GetAggregatedResult();
            // await _monitoringQueue.Save();
            await UpdateMonitoringPort(monitoringPort);

            if (reason != ReasonToSendMonitoringResult.None)
            {
                await SaveMoniResult(CreateEf(moniResult, monitoringPort, reason));
                monitoringPort.LastPreciseSavedTimestamp = DateTime.Now;
                // await _monitoringQueue.Save();
                await UpdateMonitoringPort(monitoringPort);
            }

            //await _monitoringQueue.Save();
        }
        else
            await LogFailedMeasurement(moniResult, monitoringPort);


        return moniResult;
    }

    private async Task<MoniResult> DoMeasurement(BaseRefType baseRefType, MonitoringPort monitoringPort, bool shouldChangePort = true)
    {
        _cancellationTokenSource = new CancellationTokenSource();

        if (shouldChangePort && !(await ToggleToPort(monitoringPort)))
            return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode, ReturnCode.MeasurementToggleToPortFailed);

        var baseBytes = monitoringPort.GetBaseBytes(baseRefType, _logger);
        if (baseBytes == null)
            return new MoniResult() { UserReturnCode = ReturnCode.MeasurementBaseRefNotFound, BaseRefType = baseRefType };

        _currentStep = CreateStepDto(MonitoringCurrentStep.Measure, monitoringPort, baseRefType);


        if (_cancellationTokenSource.IsCancellationRequested) // command to interrupt monitoring came while port toggling
            return new MoniResult(monitoringPort.LastMoniResult!.UserReturnCode, ReturnCode.MeasurementInterrupted);

        var result = _otdrManager
            .MeasureWithBase(new[] { _cancellationTokenSource.Token }, baseBytes, _mainCharon.GetActiveChildCharon());

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
                _logger.Info(Logs.RtuManager, accidentInSor.ToString());
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

    private MonitoringResultEf CreateEf(MoniResult moniResult, MonitoringPort monitoringPort,
        ReasonToSendMonitoringResult reason)
    {
        var dto = new MonitoringResultEf()
        {
            ReturnCode = moniResult.UserReturnCode,
            Reason = reason,
            RtuId = _config.Value.General.RtuId,
            TimeStamp = DateTime.Now,
            Serial = monitoringPort.CharonSerial,
            IsPortOnMainCharon = monitoringPort.IsPortOnMainCharon,
            OpticalPort = monitoringPort.OpticalPort,
            TraceId = monitoringPort.TraceId,
            BaseRefType = moniResult.BaseRefType,
            TraceState = moniResult.GetAggregatedResult(),
            SorBytes = moniResult.SorBytes
        };
        return dto;
    }

}