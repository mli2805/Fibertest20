using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Messaging;
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

                ProcessOnePort(monitoringPort);

                if (monitoringPort.LastMoniResult.MeasurementResult != MeasurementResult.Interrupted)
                {
                    var unused = _monitoringQueue.Dequeue();
                    _monitoringQueue.Enqueue(monitoringPort);
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
                if (monitoringPort.LastMoniResult.MeasurementResult != MeasurementResult.Success)
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

            if (moniResult.MeasurementResult == MeasurementResult.Success)
            {
                if (moniResult.GetAggregatedResult() != FiberState.Ok)
                    monitoringPort.IsBreakdownCloserThen20Km = moniResult.FirstBreakDistance < 20;

                var message = "";

                if (monitoringPort.LastTraceState == FiberState.Unknown) // 740)
                {
                    message = "First measurement on port";
                }
                else if (moniResult.GetAggregatedResult() != monitoringPort.LastTraceState)
                {
                    message = $"Trace state has changed ({monitoringPort.LastTraceState} => {moniResult.GetAggregatedResult()})";
                    monitoringPort.IsConfirmationRequired = true;
                }
                // else if (monitoringPort.IsMonitoringModeChanged)
                // {
                //     message = "Monitoring mode was changed";
                // }
                else if (_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastFastSavedTimestamp > _fastSaveTimespan)
                {
                    _rtuLog.AppendLine($"last fast saved - {monitoringPort.LastFastSavedTimestamp}, _fastSaveTimespan - {_fastSaveTimespan.TotalMinutes} minutes");
                    message = "It's time to save fast reflectogram";
                }
                monitoringPort.LastMoniResult = moniResult;
                monitoringPort.LastTraceState = moniResult.GetAggregatedResult();

                if (message != "")
                {
                    _rtuLog.AppendLine("Send by MSMQ:  " + message);
                    SendByMsmq(CreateDto(moniResult, monitoringPort));
                    monitoringPort.LastFastSavedTimestamp = DateTime.Now;
                }

                _monitoringQueue.Save();
            }
            return moniResult;
        }

        private MoniResult DoSecondMeasurement(MonitoringPort monitoringPort, bool hasFastPerformed,
            BaseRefType baseType, bool isOutOfTurnMeasurement = false)
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"MEAS. {_measurementNumber}, {baseType}, port {monitoringPort.ToStringB(_mainCharon)}");

            var moniResult = DoMeasurement(baseType, monitoringPort, !hasFastPerformed);

            if (moniResult.MeasurementResult == MeasurementResult.Success)
            {
                var message = "";
                if (isOutOfTurnMeasurement)
                    message = "It's out of turn precise measurement";
                else if (moniResult.GetAggregatedResult() != monitoringPort.LastTraceState)
                {
                    message = $"Trace state has changed ({monitoringPort.LastTraceState} => {moniResult.GetAggregatedResult()})";
                }
                // else if (monitoringPort.IsMonitoringModeChanged)
                //     message = "Monitoring mode was changed";
                else if (monitoringPort.IsConfirmationRequired)
                    message = "Accident confirmation - should be saved";
                else if (_preciseSaveTimespan != TimeSpan.Zero && DateTime.Now - monitoringPort.LastPreciseSavedTimestamp > _preciseSaveTimespan)
                    message = "It's time to save precise reflectogram";

                monitoringPort.LastPreciseMadeTimestamp = DateTime.Now;
                monitoringPort.LastMoniResult = moniResult;
                monitoringPort.LastTraceState = moniResult.GetAggregatedResult();

                if (message != "")
                {
                    _rtuLog.AppendLine("Send by MSMQ:  " + message);
                    SendByMsmq(CreateDto(moniResult, monitoringPort));
                    monitoringPort.LastPreciseSavedTimestamp = DateTime.Now;
                }

                _monitoringQueue.Save();
            }
            return moniResult;
        }

        private MoniResult DoMeasurement(BaseRefType baseRefType, MonitoringPort monitoringPort, bool shouldChangePort = true)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            if (shouldChangePort && !ToggleToPort(monitoringPort))
                return new MoniResult() { MeasurementResult = MeasurementResult.ToggleToPortFailed };

            var baseBytes = monitoringPort.GetBaseBytes(baseRefType, _rtuLog);
            if (baseBytes == null)
                return new MoniResult() { MeasurementResult = baseRefType.ToMeasurementResultProblem() };

            SendCurrentMonitoringStep(MonitoringCurrentStep.Measure, monitoringPort, baseRefType);

            _rtuIni.Write(IniSection.Monitoring, IniKey.LastMeasurementTimestamp, DateTime.Now.ToString(CultureInfo.CurrentCulture));

            if (_cancellationTokenSource.IsCancellationRequested) // command to interrupt monitoring came while port toggling
                return new MoniResult() { MeasurementResult = MeasurementResult.Interrupted };

            var result = _otdrManager.MeasureWithBase(_cancellationTokenSource, baseBytes, _mainCharon.GetActiveChildCharon());

            if (result == ReturnCode.MeasurementInterrupted)
            {
                IsMonitoringOn = false;
                SendCurrentMonitoringStep(MonitoringCurrentStep.Interrupted);
                return new MoniResult() { MeasurementResult = MeasurementResult.Interrupted };
            }

            if (result != ReturnCode.MeasurementEndedNormally)
            {
                if (RunMainCharonRecovery() != ReturnCode.Ok)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                return new MoniResult() { MeasurementResult = MeasurementResult.HardwareProblem };
            }

            _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);

            SendCurrentMonitoringStep(MonitoringCurrentStep.Analysis, monitoringPort, baseRefType);
            var buffer = _otdrManager.GetLastSorDataBuffer();
            if (_saveSorData)
                monitoringPort.SaveSorData(baseRefType, buffer, SorType.Raw, _rtuLog); // for investigations purpose
            monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Raw, _rtuLog); // for investigations purpose
            _rtuLog.AppendLine($"Measurement result ({buffer.Length} bytes).");

            try
            {
                // sometimes GetLastSorDataBuffer returns not full sor data, so
                // just to check whether OTDR still works and measurement is reliable
                if (!_otdrManager.InterOpWrapper.PrepareMeasurement(true))
                {
                    _rtuLog.AppendLine("Additional check after measurement failed!");
                    monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Error, _rtuLog); // save meas if error
                    ReInitializeDlls();
                    return new MoniResult() { MeasurementResult = MeasurementResult.HardwareProblem };
                }
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"Exception during PrepareMeasurement: {e.Message}");
            }

            MoniResult moniResult;
            try
            {
                _rtuLog.AppendLine("Start auto analysis.");
                var measBytes = _otdrManager.ApplyAutoAnalysis(buffer);
                if (_saveSorData)
                    monitoringPort.SaveSorData(baseRefType, buffer, SorType.Analysis, _rtuLog); // for investigations purpose
                monitoringPort.SaveMeasBytes(baseRefType, buffer, SorType.Analysis, _rtuLog); // 
                _rtuLog.AppendLine($"Auto analysis applied. Now sor data has {measBytes.Length} bytes.");
                moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true); // base is inserted into meas during comparison
                if (_saveSorData)
                    monitoringPort.SaveSorData(baseRefType, buffer, SorType.Meas, _rtuLog); // for investigations purpose
                monitoringPort.SaveMeasBytes(baseRefType, measBytes, SorType.Meas, _rtuLog); // so re-save meas after comparison
                moniResult.BaseRefType = baseRefType;
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"Exception during measurement analysis: {e.Message}");
                return new MoniResult() { MeasurementResult = MeasurementResult.ComparisonFailed };
            }

            LastSuccessfulMeasTimestamp = DateTime.Now;

            _rtuLog.AppendLine($"Trace state is {moniResult.GetAggregatedResult()}");
            if (moniResult.Accidents != null)
                foreach (var accidentInSor in moniResult.Accidents)
                    _rtuLog.AppendLine(accidentInSor.ToString());
            return moniResult;
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

        private MonitoringResultDto CreateDto(MoniResult moniResult, MonitoringPort monitoringPort)
        {
            var dto = new MonitoringResultDto()
            {
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

        private void SendByMsmq(MonitoringResultDto dto)
        {
            var address = _serviceIni.Read(IniSection.ServerMainAddress, IniKey.Ip, "0.0.0.0");
            var connectionString = $@"FormatName:DIRECT=TCP:{address}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);
            Message message = new Message(dto, new BinaryMessageFormatter());
            queue.Send(message, MessageQueueTransactionType.Single);
        }

        private void SendByMsmq(BopStateChangedDto dto)
        {
            _rtuLog.AppendLine("Sending OTAU state changes by MSMQ");
            var address = _serviceIni.Read(IniSection.ServerMainAddress, IniKey.Ip, "0.0.0.0");
            var connectionString = $@"FormatName:DIRECT=TCP:{address}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);
            Message message = new Message(dto, new BinaryMessageFormatter());
            queue.Send(message, MessageQueueTransactionType.Single);
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
                        RunAdditionalOtauRecovery(damagedOtau);
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
