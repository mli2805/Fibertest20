using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        private MonitoringQueue _monitoringQueue;
        private int _measurementNumber;
        private TimeSpan _preciseMakeTimespan;
        private TimeSpan _preciseSaveTimespan;
        private TimeSpan _fastSaveTimespan;

        private bool _saveSorData;
        private void RunMonitoringCycle()
        {
            _rtuIni.Write(IniSection.Monitoring, IniKey.LastMeasurementTimestamp, DateTime.Now.ToString(CultureInfo.CurrentCulture));
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, true);
            _saveSorData = _rtuIni.Read(IniSection.Monitoring, IniKey.SaveSorData, false);
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start monitoring.");
            _rtuLog.AppendLine($"_mainCharon.Serial = {_mainCharon.Serial}", 0, 3);

            if (_monitoringQueue.Count() < 1)
            {
                _rtuLog.AppendLine("There are no ports in queue for monitoring.");
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, false);
                IsMonitoringOn = false;
                return;
            }

            while (true)
            {
                _measurementNumber++;
                var monitoringPort = _monitoringQueue.Peek();

                var previousReturnCode = monitoringPort.LastMoniResult.ReturnCode;
                ProcessOnePort(monitoringPort);

                if (monitoringPort.LastMoniResult.ReturnCode != ReturnCode.MeasurementInterrupted)
                {
                    var unused = _monitoringQueue.Dequeue();
                    _monitoringQueue.Enqueue(monitoringPort);
                    _monitoringQueue.Save();
                }
                else
                {
                    // monitoringPort in memory changed
                    monitoringPort.LastMoniResult.ReturnCode = previousReturnCode;
                }

                if (!IsMonitoringOn)
                    break;
            }

            _rtuLog.AppendLine("Monitoring stopped.");

            if (!_wasMonitoringOn)
            {
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, false);
                var otdrAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, DefaultIp);
                _otdrManager.DisconnectOtdr(otdrAddress);
                IsMonitoringOn = false;
                _rtuLog.AppendLine("Rtu is turned into MANUAL mode.");
            }
        }

        private void ProcessOnePort(MonitoringPort monitoringPort)
        {
            var hasFastPerformed = false;

            var isNewTrace = monitoringPort.LastTraceState == FiberState.Unknown;
            // FAST 
            if (
                // monitoringPort.IsMonitoringModeChanged || // 740)
                (_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastFastSavedTimestamp > _fastSaveTimespan) ||
                (monitoringPort.LastTraceState == FiberState.Ok || isNewTrace))
            {
                monitoringPort.LastMoniResult = DoFastMeasurement(monitoringPort);
                if (monitoringPort.LastMoniResult.ReturnCode != ReturnCode.MeasurementEndedNormally)
                    return;
                hasFastPerformed = true;
            }

            var isTraceBroken = monitoringPort.LastTraceState != FiberState.Ok;
            var isSecondMeasurementNeeded =
                isNewTrace ||
                isTraceBroken ||
                //   monitoringPort.IsMonitoringModeChanged || // 740)
                // monitoringPort.LastPreciseMadeTimestamp == null || 
                _preciseMakeTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastPreciseMadeTimestamp > _preciseMakeTimespan;

            if (isSecondMeasurementNeeded)
            {
                // PRECISE (or ADDITIONAL)
                var baseType = (isTraceBroken && monitoringPort.IsBreakdownCloserThen20Km &&
                                monitoringPort.HasAdditionalBase())
                    ? BaseRefType.Additional
                    : BaseRefType.Precise;

                monitoringPort.LastMoniResult = DoSecondMeasurement(monitoringPort, hasFastPerformed, baseType);
            }

            monitoringPort.IsMonitoringModeChanged = false;
            monitoringPort.IsConfirmationRequired = false;
        }

        private MoniResult DoFastMeasurement(MonitoringPort monitoringPort)
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"MEAS. {_measurementNumber}, Fast, port {monitoringPort.ToStringB(_mainCharon)}");

            var moniResult = DoMeasurement(BaseRefType.Fast, monitoringPort);
            var reason = ReasonToSendMonitoringResult.None;

            if (moniResult.ReturnCode == ReturnCode.MeasurementEndedNormally)
            {
                if (moniResult.GetAggregatedResult() != FiberState.Ok)
                    monitoringPort.IsBreakdownCloserThen20Km = moniResult.FirstBreakDistance < 20;

                if (monitoringPort.LastTraceState == FiberState.Unknown) // 740)
                {
                    _rtuLog.AppendLine("First measurement on port");
                    reason |= ReasonToSendMonitoringResult.FirstMeasurementOnPort;
                }

                if (moniResult.GetAggregatedResult() != monitoringPort.LastTraceState)
                {
                    _rtuLog.AppendLine($"Trace state has changed ({monitoringPort.LastTraceState} => {moniResult.GetAggregatedResult()})");
                    reason |= ReasonToSendMonitoringResult.TraceStateChanged;
                    monitoringPort.IsConfirmationRequired = true;
                }

                // if (monitoringPort.IsMonitoringModeChanged)
                // {
                //    _rtuLog.AppendLine("Monitoring mode was changed");
                //    reason |= ReasonToSendMonitoringResult.MonitoringModeChanged;
                // }

                if (_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastFastSavedTimestamp > _fastSaveTimespan)
                {
                    _rtuLog.AppendLine($"last fast saved - {monitoringPort.LastFastSavedTimestamp}, _fastSaveTimespan - {_fastSaveTimespan.TotalMinutes} minutes");
                    _rtuLog.AppendLine("It's time to save fast reflectogram");
                    reason |= ReasonToSendMonitoringResult.TimeToRegularSave;
                }

                if (moniResult.ReturnCode != monitoringPort.LastMoniResult.ReturnCode
                    && monitoringPort.LastMoniResult.ReturnCode.IsRtuStatusEvent())
                {
                    _rtuLog.AppendLine($"previous measurement code - {monitoringPort.LastMoniResult.ReturnCode}, now - {moniResult.ReturnCode}");
                    _rtuLog.AppendLine("Problem with base ref solved");
                    reason |= ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged;
                }

                monitoringPort.LastMoniResult = moniResult;
                monitoringPort.LastTraceState = moniResult.GetAggregatedResult();
                _monitoringQueue.Save();

                if (reason != ReasonToSendMonitoringResult.None)
                {
                    SendByMsmq(CreateDto(moniResult, monitoringPort, reason));
                    monitoringPort.LastFastSavedTimestamp = DateTime.Now;
                    _monitoringQueue.Save();
                }
            }
            else //problem during measurement process 
            {
                LogFailedMeasurement(moniResult, monitoringPort);
            }
            return moniResult;
        }

        private void LogFailedMeasurement(MoniResult moniResult, MonitoringPort monitoringPort)
        {
            if (moniResult.ReturnCode.IsRtuStatusEvent())

                if (moniResult.ReturnCode != monitoringPort.LastMoniResult.ReturnCode)
                {
                    _rtuLog.AppendLine($"{monitoringPort.LastMoniResult.ReturnCode} => {moniResult.ReturnCode}");
                    _rtuLog.AppendLine("Problem with base ref occurred!");
                    SendByMsmq(CreateDto(moniResult, monitoringPort, ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged));
                }
                else
                {
                    _rtuLog.AppendLine("Problem already reported.");
                }

            // other problems provoke service restart
            // and will be discovered by user only if there are many
        }

        private MoniResult DoSecondMeasurement(MonitoringPort monitoringPort, bool hasFastPerformed,
            BaseRefType baseType, bool isOutOfTurnMeasurement = false)
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"MEAS. {_measurementNumber}, {baseType}, port {monitoringPort.ToStringB(_mainCharon)}");

            var moniResult = DoMeasurement(baseType, monitoringPort, !hasFastPerformed);

            if (moniResult.ReturnCode == ReturnCode.MeasurementEndedNormally)
            {
                var reason = ReasonToSendMonitoringResult.None;
                if (isOutOfTurnMeasurement)
                {
                    _rtuLog.AppendLine("It's out of turn precise measurement");
                    reason |= ReasonToSendMonitoringResult.OutOfTurnPreciseMeasurement;
                }

                if (moniResult.GetAggregatedResult() != monitoringPort.LastTraceState)
                {
                    _rtuLog.AppendLine($"Trace state has changed ({monitoringPort.LastTraceState} => {moniResult.GetAggregatedResult()})");
                    reason |= ReasonToSendMonitoringResult.TraceStateChanged;
                }

                //if (monitoringPort.IsMonitoringModeChanged)
                //{
                //    _rtuLog.AppendLine("Monitoring mode was changed");
                //    reason |= ReasonToSendMonitoringResult.MonitoringModeChanged;
                //}

                if (monitoringPort.IsConfirmationRequired)
                {
                    _rtuLog.AppendLine("Accident confirmation - should be saved");
                    reason |= ReasonToSendMonitoringResult.OpticalAccidentConfirmation;
                }

                if (_preciseSaveTimespan != TimeSpan.Zero &&
                         DateTime.Now - monitoringPort.LastPreciseSavedTimestamp > _preciseSaveTimespan)
                {
                    _rtuLog.AppendLine("It's time to save precise reflectogram");
                    reason |= ReasonToSendMonitoringResult.TimeToRegularSave;
                }

                if (moniResult.ReturnCode != monitoringPort.LastMoniResult.ReturnCode
                    && monitoringPort.LastMoniResult.ReturnCode.IsRtuStatusEvent())
                {
                    _rtuLog.AppendLine($"previous measurement code - {monitoringPort.LastMoniResult.ReturnCode}, now - {moniResult.ReturnCode}");
                    _rtuLog.AppendLine("Problem with base ref solved");
                    reason |= ReasonToSendMonitoringResult.MeasurementAccidentStatusChanged;
                }

                monitoringPort.LastPreciseMadeTimestamp = DateTime.Now;
                monitoringPort.LastMoniResult = moniResult;
                monitoringPort.LastTraceState = moniResult.GetAggregatedResult();
                _monitoringQueue.Save();

                if (reason != ReasonToSendMonitoringResult.None)
                {
                    SendByMsmq(CreateDto(moniResult, monitoringPort, reason));
                    monitoringPort.LastPreciseSavedTimestamp = DateTime.Now;
                    _monitoringQueue.Save();
                }

            }
            else
                LogFailedMeasurement(moniResult, monitoringPort);

            return moniResult;
        }

        private MoniResult DoMeasurement(BaseRefType baseRefType, MonitoringPort monitoringPort, bool shouldChangePort = true)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            if (shouldChangePort && !ToggleToPort(monitoringPort))
                return new MoniResult() { ReturnCode = ReturnCode.MeasurementToggleToPortFailed };

            var baseBytes = monitoringPort.GetBaseBytes(baseRefType, _rtuLog);
            if (baseBytes == null)
                return new MoniResult() { ReturnCode = ReturnCode.MeasurementBaseRefNotFound, BaseRefType = baseRefType };

            SendCurrentMonitoringStep(MonitoringCurrentStep.Measure, monitoringPort, baseRefType);


            if (_cancellationTokenSource.IsCancellationRequested) // command to interrupt monitoring came while port toggling
                return new MoniResult() { ReturnCode = ReturnCode.MeasurementInterrupted };

            var result = _otdrManager.MeasureWithBase(_cancellationTokenSource, baseBytes, _mainCharon.GetActiveChildCharon());

            switch (result)
            {
                case ReturnCode.MeasurementInterrupted:
                    IsMonitoringOn = false;
                    SendCurrentMonitoringStep(MonitoringCurrentStep.Interrupted);
                    return new MoniResult() { ReturnCode = ReturnCode.MeasurementInterrupted };

                case ReturnCode.MeasurementFailedToSetParametersFromBase:
                    // сообщить пользователю, восстановление не нужно
                    return new MoniResult() { ReturnCode = result, BaseRefType = baseRefType };

                case ReturnCode.MeasurementError:
                    if (RunMainCharonRecovery() != ReturnCode.Ok)
                        RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                    return new MoniResult() { ReturnCode = result }; // восстановление, без сообщения
            }


            _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);

            SendCurrentMonitoringStep(MonitoringCurrentStep.Analysis, monitoringPort, baseRefType);

            var buffer = _otdrManager.GetLastSorDataBuffer();
            if (_saveSorData)
                monitoringPort.SaveSorData(baseRefType, buffer, SorType.Raw, _rtuLog); // for investigations purpose
            monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Raw, _rtuLog); // for investigations purpose
            _rtuLog.AppendLine($"Measurement returned ({buffer.Length} bytes).");

            try
            {
                // sometimes GetLastSorDataBuffer returns not full sor data, so
                // just to check whether OTDR still works and measurement is reliable
                if (!_otdrManager.InterOpWrapper.PrepareMeasurement(true))
                {
                    _rtuLog.AppendLine("Additional check after measurement failed!");
                    monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Error, _rtuLog); // save meas if error
                    ReInitializeDlls();
                    return new MoniResult() { ReturnCode = ReturnCode.MeasurementHardwareProblem };
                }
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"Exception during PrepareMeasurement: {e.Message}");
                return new MoniResult() { ReturnCode = ReturnCode.MeasurementHardwareProblem };
            }

            var moniResult = AnalyzeAndCompare(baseRefType, monitoringPort, buffer, baseBytes);
            _rtuIni.Write(IniSection.Monitoring, IniKey.LastMeasurementTimestamp, DateTime.Now.ToString(CultureInfo.CurrentCulture));
            return moniResult;
        }

        private MoniResult AnalyzeAndCompare(BaseRefType baseRefType, MonitoringPort monitoringPort,
            byte[] buffer, byte[] baseBytes)
        {
            var moniResult = AnalyzeMeasurement(baseRefType, monitoringPort, buffer);
            if (moniResult.ReturnCode == ReturnCode.MeasurementAnalysisFailed) return moniResult;

            moniResult = CompareWithBase(baseRefType, monitoringPort, moniResult.SorBytes, baseBytes);
            if (moniResult.ReturnCode == ReturnCode.MeasurementComparisonFailed) return moniResult;

            LastSuccessfulMeasTimestamp = DateTime.Now;

            _rtuLog.AppendLine($"Trace state is {moniResult.GetAggregatedResult()}");
            if (moniResult.Accidents != null)
                foreach (var accidentInSor in moniResult.Accidents)
                    _rtuLog.AppendLine(accidentInSor.ToString());
            return moniResult;
        }

        private MoniResult AnalyzeMeasurement(BaseRefType baseRefType, MonitoringPort monitoringPort,
            byte[] buffer)
        {
            try
            {
                _rtuLog.AppendLine("Start auto analysis.");
                var measBytes = _otdrManager.ApplyAutoAnalysis(buffer);
                monitoringPort.SaveMeasBytes(baseRefType, measBytes, SorType.Analysis, _rtuLog);
                _rtuLog.AppendLine($"Auto analysis applied. Now sor data has {measBytes.Length} bytes.");

                return new MoniResult() { SorBytes = measBytes };
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"Exception during measurement analysis: {e.Message}");
                return new MoniResult() { ReturnCode = ReturnCode.MeasurementAnalysisFailed };
            }
        }

        private MoniResult CompareWithBase(BaseRefType baseRefType, MonitoringPort monitoringPort,
            byte[] measBytes, byte[] baseBytes)
        {
            try
            {
                _rtuLog.AppendLine("Compare with base ref.");
                // base will be inserted into meas during comparison
                var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true);

                monitoringPort.SaveMeasBytes(baseRefType, measBytes, SorType.Meas, _rtuLog); // so re-save meas after comparison
                moniResult.BaseRefType = baseRefType;
                return moniResult;
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"Exception during comparison: {e.Message}");
                return new MoniResult() { ReturnCode = ReturnCode.MeasurementComparisonFailed };
            }
        }

        private void ReInitializeDlls()
        {
            var otdrAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, DefaultIp);
            _otdrManager.DisconnectOtdr(otdrAddress);
            var otdrInitializationResult = InitializeOtdr();
            _rtuLog.AppendLine($"OTDR initialization result - {otdrInitializationResult.ToString()}");
            _serviceLog.EmptyLine();
            _serviceLog.AppendLine($"OTDR initialization result - {otdrInitializationResult.ToString()}");
        }

        private MonitoringResultDto CreateDto(MoniResult moniResult, MonitoringPort monitoringPort,
            ReasonToSendMonitoringResult reason)
        {
            var dto = new MonitoringResultDto()
            {
                ReturnCode = moniResult.ReturnCode,
                Reason = reason,
                RtuId = _id,
                TimeStamp = DateTime.Now,
                PortWithTrace = new PortWithTraceDto()
                {
                    OtauPort = new OtauPortDto()
                    {
                        Serial = monitoringPort.CharonSerial,
                        //                        OtauIp = monitoringPort.NetAddress.Ip4Address,
                        //                        OtauTcpPort = monitoringPort.NetAddress.Port,
                        IsPortOnMainCharon = monitoringPort.IsPortOnMainCharon,
                        OpticalPort = monitoringPort.OpticalPort,
                    },
                    TraceId = monitoringPort.TraceId,
                },
                BaseRefType = moniResult.BaseRefType,
                TraceState = moniResult.GetAggregatedResult(),
                SorBytes = moniResult.SorBytes
            };
            return dto;
        }


        private readonly List<DamagedOtau> _damagedOtaus = new List<DamagedOtau>();
        private bool ToggleToPort(MonitoringPort monitoringPort)
        {
            var cha = monitoringPort.IsPortOnMainCharon ? _mainCharon : _mainCharon.GetBopCharonWithLogging(monitoringPort.CharonSerial);
            // TCP port here is not important
            DamagedOtau damagedOtau = _damagedOtaus.FirstOrDefault(b => b.Ip == cha.NetAddress.Ip4Address);
            if (damagedOtau != null)
            {
                _rtuLog.AppendLine($"Port is on damaged BOP {damagedOtau.Ip}");
                if (DateTime.Now - damagedOtau.RebootStarted < _mikrotikRebootTimeout)
                {
                    _rtuLog.AppendLine($"Mikrotik {cha.NetAddress.Ip4Address} is rebooting, step to the next port");
                    return false;
                }
                else
                {
                    if (cha.OwnPortCount == 0)
                        InitializeOtau();
                }
            }

            SendCurrentMonitoringStep(MonitoringCurrentStep.Toggle, monitoringPort);

            var toggleResult = _mainCharon.SetExtendedActivePort(monitoringPort.CharonSerial, monitoringPort.OpticalPort);
            switch (toggleResult)
            {
                case CharonOperationResult.Ok:
                    {
                        _rtuLog.AppendLine("Toggled Ok.");
                        // Here TCP port is important
                        if (damagedOtau != null &&
                            damagedOtau.Ip == cha.NetAddress.Ip4Address &&
                            damagedOtau.TcpPort == cha.NetAddress.Port)
                        {
                            _rtuLog.AppendLine($"OTAU {cha.NetAddress.ToStringA()} recovered");
                            var mikrotikRebootAttemptsBeforeNotification = _rtuIni.Read(IniSection.Recovering, IniKey.MikrotikRebootAttemptsBeforeNotification, 3);
                            if (damagedOtau.RebootAttempts >= mikrotikRebootAttemptsBeforeNotification)
                            {
                                _rtuLog.AppendLine("Send notification to server.");
                                var dto = new BopStateChangedDto()
                                {
                                    RtuId = _id,
                                    Serial = monitoringPort.CharonSerial,
                                    OtauIp = cha.NetAddress.Ip4Address,
                                    TcpPort = cha.NetAddress.Port,
                                    IsOk = true,
                                };
                                SendByMsmq(dto);
                            }
                            _damagedOtaus.Remove(damagedOtau);
                        }

                        return true;
                    }
                case CharonOperationResult.MainOtauError:
                    {
                        LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.ErrorTogglePort);
                        if (RunMainCharonRecovery() != ReturnCode.Ok)
                            RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                        return false;
                    }
                case CharonOperationResult.AdditionalOtauError:
                    {
                        if (damagedOtau == null)
                        {
                            damagedOtau = new DamagedOtau(cha.NetAddress.Ip4Address, cha.NetAddress.Port, monitoringPort.CharonSerial);
                            _damagedOtaus.Add(damagedOtau);
                        }
                        RunBopRecovery(damagedOtau);
                        return false;
                    }
                default:
                    {
                        _rtuLog.AppendLine(_mainCharon.LastErrorMessage);
                        return false;
                    }
            }
        }

    }
}
