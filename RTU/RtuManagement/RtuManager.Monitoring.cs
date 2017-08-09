using System;
using System.Collections.Generic;
using System.Linq;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
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

        private void RunMonitoringCycle(bool shouldSendMonitoringStarted)
        {
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start monitoring.");

            if (_monitoringQueue.Count < 1)
            {
                _rtuLog.AppendLine("There are no ports in queue for monitoring.");
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
                new R2DWcfManager(_serverIp, _serviceIni, _serviceLog).SendMonitoringStarted(new MonitoringStartedDto() {RtuId = _id, IsSuccessful = false });
                IsMonitoringOn = false;
                return;
            }

            while (true)
            {
                _measurementNumber++;
                if (shouldSendMonitoringStarted)
                {
                    new R2DWcfManager(_serverIp, _serviceIni, _serviceLog).SendMonitoringStarted(new MonitoringStartedDto() { RtuId = _id, IsSuccessful = true });
                    shouldSendMonitoringStarted = false;
                }

                var extendedPort = _monitoringQueue.Dequeue();
                _monitoringQueue.Enqueue(extendedPort);

                _rtuLog.EmptyLine();
                ProcessOnePort(extendedPort);

                lock (_obj)
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
            new R2DWcfManager(_serverIp, _serviceIni, _serviceLog).SendMonitoringStopped(new MonitoringStoppedDto() { RtuId = _id, IsSuccessful = true });
        }

        private void ProcessOnePort(ExtendedPort extendedPort)
        {
            var hasFastPerformed = false;
            if (extendedPort.State == PortMeasResult.Ok || extendedPort.State == PortMeasResult.Unknown)
            {
                // FAST 
                _rtuLog.AppendLine($"MEAS. {_measurementNumber} port {_mainCharon.GetBopPortString(extendedPort)}, Fast");
                var fastMoniResult = DoMeasurement(BaseRefType.Fast, extendedPort);
                if (fastMoniResult == null)
                    return;
                hasFastPerformed = true;
                if (GetPortState(fastMoniResult) != extendedPort.State ||
                    (extendedPort.LastFastSavedTimestamp - DateTime.Now) > _fastSaveTimespan)
                {
                    SendMonitoringResultToDataCenter(fastMoniResult);
                    extendedPort.LastFastSavedTimestamp = DateTime.Now;
                    extendedPort.State = GetPortState(fastMoniResult);
                    if (extendedPort.State == PortMeasResult.BrokenByFast)
                        extendedPort.IsBreakdownCloserThen20Km = fastMoniResult.FirstBreakDistance < 20;
                }
            }

            var isTraceBroken = extendedPort.State == PortMeasResult.BrokenByFast ||
                                extendedPort.State == PortMeasResult.BrokenByPrecise;
            var isSecondMeasurementNeeded = isTraceBroken ||
                                            (DateTime.Now - extendedPort.LastPreciseMadeTimestamp) >
                                            _preciseMakeTimespan;

            if (!isSecondMeasurementNeeded)
                return;
            if (hasFastPerformed)
                _rtuLog.EmptyLine();

            // PRECISE (or ADDITIONAL)
            var baseType = (isTraceBroken && extendedPort.IsBreakdownCloserThen20Km && extendedPort.HasAdditionalBase())
                ? BaseRefType.Additional
                : BaseRefType.Precise;
            var message = $"MEAS. {_measurementNumber} port {_mainCharon.GetBopPortString(extendedPort)}, {baseType}";
            message += hasFastPerformed ? " (confirmation)" : "";
            _rtuLog.AppendLine(message);
            var moniResult = DoMeasurement(baseType, extendedPort, !hasFastPerformed);
            if (moniResult == null)
                return;
            extendedPort.LastPreciseMadeTimestamp = DateTime.Now;
            if (GetPortState(moniResult) != extendedPort.State ||
                (DateTime.Now - extendedPort.LastPreciseSavedTimestamp) > _preciseSaveTimespan)
            {
                SendMonitoringResultToDataCenter(moniResult);
                extendedPort.LastPreciseSavedTimestamp = DateTime.Now;
                extendedPort.State = GetPortState(moniResult);
            }
        }

        private MoniResult DoMeasurement(BaseRefType baseRefType, ExtendedPort extendedPort, bool isPortChanged = true)
        {
            if (isPortChanged && !ToggleToPort(extendedPort))
                return null;
            var baseBytes = extendedPort.GetBaseBytes(baseRefType, _rtuLog);
            if (baseBytes == null)
                return null;
            SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Measure, extendedPort, baseRefType);
            if (!_otdrManager.MeasureWithBase(baseBytes, _mainCharon.GetActiveChildCharon()))
                return null;
            if (_isMonitoringCancelled)
            {
                SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Interrupted, extendedPort, baseRefType);
                return null;
            }
            SendCurrentMonitoringStep(RtuCurrentMonitoringStep.Analysis, extendedPort, baseRefType);
            var measBytes = _otdrManager.ApplyAutoAnalysis(_otdrManager.GetLastSorDataBuffer()); // is ApplyAutoAnalysis necessary ?
            var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true); // base is inserted into meas during comparison
            extendedPort.SaveMeasBytes(baseRefType, measBytes); // so re-save meas after comparison
            return moniResult;
        }

        private List<DamagedOtau> _damagedOtaus = new List<DamagedOtau>();
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
            var toggleResult = _mainCharon.SetExtendedActivePort(extendedPort.NetAddress, extendedPort.Port);
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
