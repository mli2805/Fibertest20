using System;
using System.Collections.Generic;
using System.Linq;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private bool _hasNewSettings;
        private Queue<ExtendedPort> _monitoringQueue;
        private int _measurementNumber;
        private TimeSpan _preciseMakeTimespan;
        private TimeSpan _preciseSaveTimespan;
        private TimeSpan _fastSaveTimespan;

        private void RunMonitoringCycle(bool shouldSendMessageMonitoringStarted)
        {
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start monitoring.");

            if (_monitoringQueue.Count < 1)
            {
                _rtuLog.AppendLine("There are no ports in queue for monitoring.");
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
                new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendMonitoringStarted(new MonitoringStartedDto() { RtuId = _id, IsSuccessful = false });
                IsMonitoringOn = false;
                return;
            }

            while (true)
            {
                _measurementNumber++;
                if (shouldSendMessageMonitoringStarted)
                {
                    new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendMonitoringStarted(new MonitoringStartedDto() { RtuId = _id, IsSuccessful = true });
                    shouldSendMessageMonitoringStarted = false;
                }

                var extendedPort = _monitoringQueue.Dequeue();
                _monitoringQueue.Enqueue(extendedPort);

                ProcessOnePort(extendedPort);

                lock (_isMonitoringCancelledLocker)
                {
                    if (_isMonitoringCancelled)
                        break;
                    if (_hasNewSettings)
                    {
                        ApplyChangeSettings();
                        _hasNewSettings = false;
                    }
                }
            }

            _rtuLog.AppendLine("Monitoring stopped.");
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
            var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            _otdrManager.DisconnectOtdr(otdrAddress);
            IsMonitoringOn = false;
            _isMonitoringCancelled = false;
            _rtuLog.AppendLine("Rtu is turned into MANUAL mode.");
            new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendMonitoringStopped(new MonitoringStoppedDto() { RtuId = _id, IsSuccessful = true });
        }

        private MoniResult DoFastMeasurement(ExtendedPort extendedPort)
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"MEAS. {_measurementNumber} port {_mainCharon.GetBopPortString(extendedPort)}, Fast");

            var moniResult = DoMeasurement(BaseRefType.Fast, extendedPort);
            if (moniResult != null)
            {
                if (moniResult.GetAggregatedResult() != FiberState.Ok)
                    extendedPort.IsBreakdownCloserThen20Km = moniResult.FirstBreakDistance < 20;

                var message = "";
//                if (extendedPort.LastMoniResult == null)
//                    message = "First measurement after restart";
//                else 
                if (extendedPort.LastMoniResult.GetAggregatedResult() != moniResult.GetAggregatedResult())
                {
                    message = "Trace state has changed";
                    SaveTraceStateInFile();
                }
                else if (DateTime.Now - extendedPort.LastFastSavedTimestamp > _fastSaveTimespan)
                    message = "It's time to save fast reflectogram";

                if (message != "")
                {
                    _rtuLog.AppendLine(message);
                    PlaceMonitoringResultInSendingQueue(moniResult, extendedPort);
                    extendedPort.LastFastSavedTimestamp = DateTime.Now;
                }
            }
            return moniResult;
        }

        private MoniResult DoSecondMeasurement(ExtendedPort extendedPort, bool hasFastPerformed, BaseRefType baseType)
        {
            _rtuLog.EmptyLine();
            var caption = $"MEAS. {_measurementNumber} port {_mainCharon.GetBopPortString(extendedPort)}, {baseType}";
            caption += hasFastPerformed ? " (confirmation)" : "";
            _rtuLog.AppendLine(caption);

            var moniResult = DoMeasurement(baseType, extendedPort, !hasFastPerformed);
            if (moniResult != null)
            {
                var message = "";
//                if (extendedPort.LastPreciseMadeTimestamp == null)
//                    message = "First measurement after restart";
//                else 
                if (extendedPort.LastMoniResult.GetAggregatedResult() != moniResult.GetAggregatedResult())
                {
                    message = "Trace state has changed";
                    SaveTraceStateInFile();
                }
                else if (DateTime.Now - extendedPort.LastPreciseSavedTimestamp > _preciseSaveTimespan)
                    message = "It's time to save precise reflectogram";

                if (message != "")
                {
                    _rtuLog.AppendLine(message);
                    PlaceMonitoringResultInSendingQueue(moniResult, extendedPort);
                    extendedPort.LastPreciseSavedTimestamp = DateTime.Now;
                }

                extendedPort.LastPreciseMadeTimestamp = DateTime.Now;
            }
            return moniResult;
        }

        private void SaveTraceStateInFile()
        {
            
        }

        private void ProcessOnePort(ExtendedPort extendedPort)
        {
            var hasFastPerformed = false;
            if (extendedPort.LastMoniResult == null ||
                extendedPort.LastMoniResult.GetAggregatedResult() == FiberState.Ok)
            {
                // FAST 
                extendedPort.LastMoniResult = DoFastMeasurement(extendedPort);
                if (extendedPort.LastMoniResult == null)
                    return;
                hasFastPerformed = true;
            }

            var isTraceBroken = extendedPort.LastMoniResult.GetAggregatedResult() != FiberState.Ok;
            var isSecondMeasurementNeeded = isTraceBroken ||
                                            (DateTime.Now - extendedPort.LastPreciseMadeTimestamp) >
                                            _preciseMakeTimespan;

            if (isSecondMeasurementNeeded)
            {
                // PRECISE (or ADDITIONAL)
                var baseType = (isTraceBroken && extendedPort.IsBreakdownCloserThen20Km &&
                                extendedPort.HasAdditionalBase())
                    ? BaseRefType.Additional
                    : BaseRefType.Precise;

                extendedPort.LastMoniResult = DoSecondMeasurement(extendedPort, hasFastPerformed, baseType);
            }
        }

        private MoniResult DoMeasurement(BaseRefType baseRefType, ExtendedPort extendedPort, bool shouldChangePort = true)
        {
            if (shouldChangePort && !ToggleToPort(extendedPort))
                return null;
            var baseBytes = extendedPort.GetBaseBytes(baseRefType, _rtuLog);
            if (baseBytes == null)
                return null;
            SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Measure, extendedPort, baseRefType);
            if (!_otdrManager.MeasureWithBase(baseBytes, _mainCharon.GetActiveChildCharon()))
            {                                 // Error 814 during measurement prepare
                RunMainCharonRecovery();
                return null;
            }
            if (_isMonitoringCancelled)
            {
                SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Interrupted, extendedPort, baseRefType);
                return null;
            }
            SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Analysis, extendedPort, baseRefType);
            var measBytes = _otdrManager.ApplyAutoAnalysis(_otdrManager.GetLastSorDataBuffer()); // is ApplyAutoAnalysis necessary ?
            var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true); // base is inserted into meas during comparison
            extendedPort.SaveMeasBytes(baseRefType, measBytes); // so re-save meas after comparison
            moniResult.BaseRefType = baseRefType;

            LastSuccessfullMeasTimestamp = DateTime.Now;

            _rtuLog.AppendLine($"Trace state is {moniResult.GetAggregatedResult()}");
            return moniResult;
        }

        private void PlaceMonitoringResultInSendingQueue(MoniResult moniResult, ExtendedPort extendedPort)
        {
            var dto = new MonitoringResultDto()
            {
                RtuId = _id,
                TimeStamp = DateTime.Now,
                OtauPort = new OtauPortDto()
                {
                    OtauIp = extendedPort.NetAddress.Ip4Address,
                    OtauTcpPort = extendedPort.NetAddress.Port,
                    IsPortOnMainCharon = extendedPort.IsPortOnMainCharon,
                    OpticalPort = extendedPort.OpticalPort,
                },
                BaseRefType = moniResult.BaseRefType,
                TraceState = moniResult.GetAggregatedResult(),
                SorData = moniResult.SorBytes
            };
            var moniResultOnDisk = new MoniResultOnDisk(Guid.NewGuid(), dto, _serviceLog);
            moniResultOnDisk.Save();
            _rtuLog.AppendLine($"There are {QueueOfMoniResultsOnDisk.Count} moniresults in the queue");
            QueueOfMoniResultsOnDisk.Enqueue(moniResultOnDisk);
            _rtuLog.AppendLine("Monitoring result is placed in sending queue");
            _rtuLog.AppendLine($"There are {QueueOfMoniResultsOnDisk.Count} moniresults in the queue");
        }

        private readonly List<DamagedOtau> _damagedOtaus = new List<DamagedOtau>();
        private bool ToggleToPort(ExtendedPort extendedPort)
        {
            var otauIp = extendedPort.NetAddress.Ip4Address;
            DamagedOtau damagedOtau = extendedPort.NetAddress.Equals(_mainCharon.NetAddress)
                ? null
                : _damagedOtaus.FirstOrDefault(b => b.Ip == otauIp);
            if (damagedOtau != null && (DateTime.Now - damagedOtau.RebootStarted < _mikrotikRebootTimeout))
            {
                _rtuLog.AppendLine("Mikrotik is rebooting, step to the next port");
                return false;
            }

            SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Toggle, extendedPort);
            var toggleResult = _mainCharon.SetExtendedActivePort(extendedPort.NetAddress, extendedPort.OpticalPort);
            switch (toggleResult)
            {
                case CharonOperationResult.Ok:
                    {
                        if (damagedOtau != null)
                            damagedOtau.RebootAttempts = 0;
                        _rtuIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);
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
                        RunAdditionalOtauRecovery(damagedOtau, otauIp);
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
