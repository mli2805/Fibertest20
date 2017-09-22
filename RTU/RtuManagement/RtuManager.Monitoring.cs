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
        private MonitoringQueue _monitoringQueue;
        private int _measurementNumber;
        private TimeSpan _preciseMakeTimespan;
        private TimeSpan _preciseSaveTimespan;
        private TimeSpan _fastSaveTimespan;

        private void RunMonitoringCycle(bool shouldSendMessageMonitoringStarted)
        {
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start monitoring.");

            if (_monitoringQueue.Count() < 1)
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

        private MoniResult DoFastMeasurement(MonitorigPort monitorigPort)
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"MEAS. {_measurementNumber} port {monitorigPort.ToStringB(_mainCharon)}, Fast");

            var moniResult = DoMeasurement(BaseRefType.Fast, monitorigPort);
            if (moniResult != null)
            {
                if (moniResult.GetAggregatedResult() != FiberState.Ok)
                    monitorigPort.IsBreakdownCloserThen20Km = moniResult.FirstBreakDistance < 20;

                var message = "";
                if (monitorigPort.LastTraceState != moniResult.GetAggregatedResult())
                {
                    message = "Trace state has changed";
                    monitorigPort.LastTraceState = moniResult.GetAggregatedResult();
                    _monitoringQueue.Save();
                }
                else if (DateTime.Now - monitorigPort.LastFastSavedTimestamp > _fastSaveTimespan)
                    message = "It's time to save fast reflectogram";

                if (message != "")
                {
                    _rtuLog.AppendLine(message);
                    PlaceMonitoringResultInSendingQueue(moniResult, monitorigPort);
                    monitorigPort.LastFastSavedTimestamp = DateTime.Now;
                }
            }
            return moniResult;
        }

        private MoniResult DoSecondMeasurement(MonitorigPort monitorigPort, bool hasFastPerformed, BaseRefType baseType)
        {
            _rtuLog.EmptyLine();
            var caption = $"MEAS. {_measurementNumber} port {monitorigPort.ToStringB(_mainCharon)}, {baseType}";
            caption += hasFastPerformed ? " (confirmation)" : "";
            _rtuLog.AppendLine(caption);

            var moniResult = DoMeasurement(baseType, monitorigPort, !hasFastPerformed);
            if (moniResult != null)
            {
                var message = "";
                if (monitorigPort.LastMoniResult.GetAggregatedResult() != moniResult.GetAggregatedResult())
                {
                    message = "Trace state has changed";
                    monitorigPort.LastTraceState = moniResult.GetAggregatedResult();
                    _monitoringQueue.Save();
                }
                else if (DateTime.Now - monitorigPort.LastPreciseSavedTimestamp > _preciseSaveTimespan)
                    message = "It's time to save precise reflectogram";

                if (message != "")
                {
                    _rtuLog.AppendLine(message);
                    PlaceMonitoringResultInSendingQueue(moniResult, monitorigPort);
                    monitorigPort.LastPreciseSavedTimestamp = DateTime.Now;
                }

                monitorigPort.LastPreciseMadeTimestamp = DateTime.Now;
            }
            return moniResult;
        }

       private void ProcessOnePort(MonitorigPort monitorigPort)
        {
            var hasFastPerformed = false;
            if (monitorigPort.LastMoniResult == null ||
                monitorigPort.LastMoniResult.GetAggregatedResult() == FiberState.Ok)
            {
                // FAST 
                monitorigPort.LastMoniResult = DoFastMeasurement(monitorigPort);
                if (monitorigPort.LastMoniResult == null)
                    return;
                hasFastPerformed = true;
            }

            var isTraceBroken = monitorigPort.LastMoniResult.GetAggregatedResult() != FiberState.Ok;
            var isSecondMeasurementNeeded = isTraceBroken ||
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

        private MoniResult DoMeasurement(BaseRefType baseRefType, MonitorigPort monitorigPort, bool shouldChangePort = true)
        {
            if (shouldChangePort && !ToggleToPort(monitorigPort))
                return null;
            var baseBytes = monitorigPort.GetBaseBytes(baseRefType, _rtuLog);
            if (baseBytes == null)
                return null;
            SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Measure, monitorigPort, baseRefType);
            if (!_otdrManager.MeasureWithBase(baseBytes, _mainCharon.GetActiveChildCharon()))
            {                                 // Error 814 during measurement prepare
                RunMainCharonRecovery();
                return null;
            }
            if (_isMonitoringCancelled)
            {
                SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Interrupted, monitorigPort, baseRefType);
                return null;
            }
            SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Analysis, monitorigPort, baseRefType);
            var measBytes = _otdrManager.ApplyAutoAnalysis(_otdrManager.GetLastSorDataBuffer()); // is ApplyAutoAnalysis necessary ?
            var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true); // base is inserted into meas during comparison
            monitorigPort.SaveMeasBytes(baseRefType, measBytes); // so re-save meas after comparison
            moniResult.BaseRefType = baseRefType;

            LastSuccessfullMeasTimestamp = DateTime.Now;

            _rtuLog.AppendLine($"Trace state is {moniResult.GetAggregatedResult()}");
            return moniResult;
        }

        private void PlaceMonitoringResultInSendingQueue(MoniResult moniResult, MonitorigPort monitorigPort)
        {
            var dto = new MonitoringResultDto()
            {
                RtuId = _id,
                TimeStamp = DateTime.Now,
                OtauPort = new OtauPortDto()
                {
                    OtauIp = monitorigPort.NetAddress.Ip4Address,
                    OtauTcpPort = monitorigPort.NetAddress.Port,
                    IsPortOnMainCharon = monitorigPort.IsPortOnMainCharon,
                    OpticalPort = monitorigPort.OpticalPort,
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
        private bool ToggleToPort(MonitorigPort monitorigPort)
        {
            var otauIp = monitorigPort.NetAddress.Ip4Address;
            DamagedOtau damagedOtau = monitorigPort.NetAddress.Equals(_mainCharon.NetAddress)
                ? null
                : _damagedOtaus.FirstOrDefault(b => b.Ip == otauIp);
            if (damagedOtau != null && (DateTime.Now - damagedOtau.RebootStarted < _mikrotikRebootTimeout))
            {
                _rtuLog.AppendLine("Mikrotik is rebooting, step to the next port");
                return false;
            }

            SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Toggle, monitorigPort);
            var toggleResult = _mainCharon.SetExtendedActivePort(monitorigPort.NetAddress, monitorigPort.OpticalPort);
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
