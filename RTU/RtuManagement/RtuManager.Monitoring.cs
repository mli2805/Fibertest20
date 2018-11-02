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

        private void RunMonitoringCycle()
        {
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start monitoring.");

            if (_monitoringQueue.Count() < 1)
            {
                _rtuLog.AppendLine("There are no ports in queue for monitoring.");
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
                IsMonitoringOn = false;
                return;
            }

            while (true)
            {
                _measurementNumber++;
                var monitorigPort = _monitoringQueue.Dequeue();
                _monitoringQueue.Enqueue(monitorigPort);

                ProcessOnePort(monitorigPort);

                if (!IsMonitoringOn)
                    break;
            }

            _rtuLog.AppendLine("Monitoring stopped.");
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
            var otdrAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, DefaultIp);
            _otdrManager.DisconnectOtdr(otdrAddress);
            IsMonitoringOn = false;
            _rtuLog.AppendLine("Rtu is turned into MANUAL mode.");
        }

        private void ProcessOnePort(MonitorigPort monitorigPort)
        {
            var hasFastPerformed = false;

            // FAST 
            if (monitorigPort.MonitoringModeChangedFlag ||
                (_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitorigPort.LastFastSavedTimestamp > _fastSaveTimespan) ||
                monitorigPort.LastTraceState == FiberState.Ok)
            {
                monitorigPort.LastMoniResult = DoFastMeasurement(monitorigPort);
                if (monitorigPort.LastMoniResult == null)
                    return;
                hasFastPerformed = true;
            }

            var isTraceBroken = monitorigPort.LastTraceState != FiberState.Ok;
            var isSecondMeasurementNeeded = isTraceBroken ||
                                            monitorigPort.MonitoringModeChangedFlag ||
                                            (DateTime.Now - monitorigPort.LastPreciseMadeTimestamp) >
                                            _preciseMakeTimespan;

            if (isSecondMeasurementNeeded)
            {
                // PRECISE (or ADDITIONAL)
                var baseType = (isTraceBroken && monitorigPort.IsBreakdownCloserThen20Km &&
                                monitorigPort.HasAdditionalBase())
                    ? BaseRefType.Additional
                    : BaseRefType.Precise;

                monitorigPort.LastMoniResult = DoSecondMeasurement(monitorigPort, hasFastPerformed, baseType);
            }
        }

        private MoniResult DoFastMeasurement(MonitorigPort monitorigPort)
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"MEAS. {_measurementNumber}, Fast, port {monitorigPort.ToStringB(_mainCharon)}");

            var moniResult = DoMeasurement(BaseRefType.Fast, monitorigPort);

            if (moniResult != null)
            {
                if (moniResult.GetAggregatedResult() != FiberState.Ok)
                    monitorigPort.IsBreakdownCloserThen20Km = moniResult.FirstBreakDistance < 20;

                var message = "";
                if (monitorigPort.LastTraceState != moniResult.GetAggregatedResult())
                {
                    message = $"Trace state has changed ({monitorigPort.LastTraceState} => {moniResult.GetAggregatedResult()})";
                    monitorigPort.LastTraceState = moniResult.GetAggregatedResult();
                }
                else if (monitorigPort.MonitoringModeChangedFlag)
                {
                    message = "Monitoring mode was changed";
                }
                else if (_fastSaveTimespan != TimeSpan.Zero && DateTime.Now - monitorigPort.LastFastSavedTimestamp > _fastSaveTimespan)
                {
                    _rtuLog.AppendLine($"last fast saved - {monitorigPort.LastFastSavedTimestamp}, _fastSaveTimespan - {_fastSaveTimespan.TotalMinutes} minutes");
                    message = "It's time to save fast reflectogram";
                }

                if (message != "")
                {
                    _rtuLog.AppendLine("Send by MSMQ:  " + message);
                    SendByMsmq(CreateDto(moniResult, monitorigPort));
                    monitorigPort.LastFastSavedTimestamp = DateTime.Now;
                }

                _monitoringQueue.Save();
            }
            return moniResult;
        }

        private MoniResult DoSecondMeasurement(MonitorigPort monitorigPort, bool hasFastPerformed, BaseRefType baseType, bool isOutOfTurnMeasurement = false)
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"MEAS. {_measurementNumber}, {baseType}, port {monitorigPort.ToStringB(_mainCharon)}");

            var moniResult = DoMeasurement(baseType, monitorigPort, !hasFastPerformed);

            if (moniResult != null)
            {
                monitorigPort.LastPreciseMadeTimestamp = DateTime.Now;

                var message = "";
                if (isOutOfTurnMeasurement)
                {
                    message = "It's out of turn precise measurement";
                }
                else if (monitorigPort.LastTraceState != moniResult.GetAggregatedResult())
                {
                    message = "Trace state has changed";
                    monitorigPort.LastTraceState = moniResult.GetAggregatedResult();
                }
                else if (monitorigPort.MonitoringModeChangedFlag)
                {
                    message = "Monitoring mode was changed";
                    monitorigPort.MonitoringModeChangedFlag = false;
                }
                else if (hasFastPerformed)
                {
                    message = "Accident confirmation - should be saved";
                }
                else if (_preciseSaveTimespan != TimeSpan.Zero && DateTime.Now - monitorigPort.LastPreciseSavedTimestamp > _preciseSaveTimespan)
                    message = "It's time to save precise reflectogram";

                if (message != "")
                {
                    _rtuLog.AppendLine("Send by MSMQ:  " + message);
                    SendByMsmq(CreateDto(moniResult, monitorigPort));
                    monitorigPort.LastPreciseSavedTimestamp = DateTime.Now;
                }

                _monitoringQueue.Save();
            }
            return moniResult;
        }

        private MoniResult DoMeasurement(BaseRefType baseRefType, MonitorigPort monitorigPort, bool shouldChangePort = true)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _rtuIni.Write(IniSection.Monitoring, IniKey.LastMeasurementTimestamp, DateTime.Now.ToString(CultureInfo.CurrentCulture));

            if (shouldChangePort && !ToggleToPort(monitorigPort))
                return null;

            var baseBytes = monitorigPort.GetBaseBytes(baseRefType, _rtuLog);
            if (baseBytes == null)
                return null;

            SendCurrentMonitoringStep(MonitoringCurrentStep.Measure, monitorigPort, baseRefType);

            if (_cancellationTokenSource.IsCancellationRequested) // command to interrupt monitoring came while port toggling
                return null;

            var result = _otdrManager.MeasureWithBase(_cancellationTokenSource, baseBytes, _mainCharon.GetActiveChildCharon());

            if (result == ReturnCode.MeasurementInterrupted)
            {
                IsMonitoringOn = false;
                SendCurrentMonitoringStep(MonitoringCurrentStep.Interrupted);
                return null;
            }

            if (result != ReturnCode.MeasurementEndedNormally)
            {                                 // IIT Error 814 during measurement prepare
                RunMainCharonRecovery();
                return null;
            }

            _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);
            
            SendCurrentMonitoringStep(MonitoringCurrentStep.Analysis, monitorigPort, baseRefType);
            var measBytes = _otdrManager.ApplyAutoAnalysis(_otdrManager.GetLastSorDataBuffer()); // is ApplyAutoAnalysis necessary ?
            var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true); // base is inserted into meas during comparison
            monitorigPort.SaveMeasBytes(baseRefType, measBytes); // so re-save meas after comparison
            moniResult.BaseRefType = baseRefType;

            LastSuccessfullMeasTimestamp = DateTime.Now;

            _rtuLog.AppendLine($"Trace state is {moniResult.GetAggregatedResult()}");
            return moniResult;
        }

        private MonitoringResultDto CreateDto(MoniResult moniResult, MonitorigPort monitorigPort)
        {
            var dto = new MonitoringResultDto()
            {
                RtuId = _id,
                TimeStamp = DateTime.Now,
                PortWithTrace = new PortWithTraceDto()
                {
                    OtauPort = new OtauPortDto()
                    {
                        OtauIp = monitorigPort.NetAddress.Ip4Address,
                        OtauTcpPort = monitorigPort.NetAddress.Port,
                        IsPortOnMainCharon = monitorigPort.IsPortOnMainCharon,
                        OpticalPort = monitorigPort.OpticalPort,
                    },
                    TraceId = monitorigPort.TraceId,
                },
                BaseRefType = moniResult.BaseRefType,
                TraceState = moniResult.GetAggregatedResult(),
                SorBytes = moniResult.SorBytes
            };
            return dto;
        }

        private void SendByMsmq(MonitoringResultDto dto)
        {
            var address = _serviceIni.Read(IniSection.ServerMainAddress, IniKey.Ip, "192.168.96.0");
            var connectionString = $@"FormatName:DIRECT=TCP:{address}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);
            Message message = new Message(dto, new BinaryMessageFormatter());
            queue.Send(message, MessageQueueTransactionType.Single);
        }

        private void SendByMsmq(BopStateChangedDto dto)
        {
            _rtuLog.AppendLine("Sending OTAU state changes by MSMQ");
            var address = _serviceIni.Read(IniSection.ServerMainAddress, IniKey.Ip, "192.168.96.0");
            var connectionString = $@"FormatName:DIRECT=TCP:{address}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);
            Message message = new Message(dto, new BinaryMessageFormatter());
            queue.Send(message, MessageQueueTransactionType.Single);
        }

        private readonly List<DamagedOtau> _damagedOtaus = new List<DamagedOtau>();
        private bool ToggleToPort(MonitorigPort monitorigPort)
        {
                                                                        // TCP port here is not important
            DamagedOtau damagedOtau = _damagedOtaus.FirstOrDefault(b => b.Ip == monitorigPort.NetAddress.Ip4Address);
            if (damagedOtau != null)
            {
                _rtuLog.AppendLine($"Port is on damaged BOP {damagedOtau.Ip}");
                if (DateTime.Now - damagedOtau.RebootStarted < _mikrotikRebootTimeout)
                {
                    _rtuLog.AppendLine($"Mikrotik {monitorigPort.NetAddress.Ip4Address} is rebooting, step to the next port");
                    return false;
                }
                else
                {
                    var ch = _mainCharon.GetBopCharonWithLogging(monitorigPort.NetAddress);
                    if (ch.OwnPortCount == 0)
                        InitializeOtau();
                }
            }

            SendCurrentMonitoringStep(MonitoringCurrentStep.Toggle, monitorigPort);

            var toggleResult = _mainCharon.SetExtendedActivePort(monitorigPort.NetAddress, monitorigPort.OpticalPort);
            switch (toggleResult)
            {
                case CharonOperationResult.Ok:
                    {
                        _rtuLog.AppendLine("Toggled Ok.");
                        // Here TCP port is important
                        if (damagedOtau != null &&
                            damagedOtau.Ip == monitorigPort.NetAddress.Ip4Address &&
                            damagedOtau.TcpPort == monitorigPort.NetAddress.Port)
                        {
                            _rtuLog.AppendLine($"OTAU {monitorigPort.NetAddress.ToStringA()} recovered, send notification to server.");
                            var dto = new BopStateChangedDto()
                            {
                                RtuId = _id,
                                OtauIp = monitorigPort.NetAddress.Ip4Address,
                                TcpPort = monitorigPort.NetAddress.Port,
                                IsOk = true,
                            };
                            SendByMsmq(dto);
                            _damagedOtaus.Remove(damagedOtau);
                        }
                   
                        return true;
                    }
                case CharonOperationResult.MainOtauError:
                    {
                        LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.ErrorTogglePort);
                        RunMainCharonRecovery();
                        return false;
                    }
                case CharonOperationResult.AdditionalOtauError:
                    {
                        if (damagedOtau == null)
                        {
                            damagedOtau = new DamagedOtau(monitorigPort.NetAddress.Ip4Address, monitorigPort.NetAddress.Port);
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
