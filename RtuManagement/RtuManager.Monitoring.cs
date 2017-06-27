using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private Queue<ExtendedPort> _monitoringQueue;
        private int _measurementNumber;
        private TimeSpan _preciseMakeTimespan;
        private TimeSpan _preciseSaveTimespan;
        private TimeSpan _fastSaveTimespan;

        private void RunMonitoringCycle()
        {
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start monitoring.");

            if (_monitoringQueue.Count < 1)
            {
                _rtuLog.AppendLine("There are no ports in queue for monitoring.");
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
                IsMonitoringOn = false;
                return;
            }
            while (true)
            {
                _measurementNumber++;

                var extendedPort = _monitoringQueue.Dequeue();
                _monitoringQueue.Enqueue(extendedPort);

                _rtuLog.EmptyLine();
                ProcessOnePort(extendedPort);

                lock (_obj)
                {
                    if (_isMonitoringCancelled)
                        break;
                }
            }
            _rtuLog.AppendLine("Monitoring stopped.");
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
                    SendMoniResult(fastMoniResult);
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
                SendMoniResult(moniResult);
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
            if (!_otdrManager.MeasureWithBase(baseBytes, _mainCharon.GetActiveChildCharon()))
                return null;
            if (_isMonitoringCancelled)
            {
                return null;
            }
            var measBytes = _otdrManager.ApplyAutoAnalysis(_otdrManager.GetLastSorDataBuffer()); // is ApplyAutoAnalysis necessary ?
            var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, ref measBytes, true); // base is inserted into meas during comparison
            extendedPort.SaveMeasBytes(baseRefType, measBytes); // so re-save meas after comparison
            return moniResult;
        }

        private List<BopProblem> _bopProblems = new List<BopProblem>();
        private bool ToggleToPort(ExtendedPort extendedPort)
        {
            var bopIp = extendedPort.NetAddress.Ip4Address;
            BopProblem bopProblem = extendedPort.NetAddress.Equals(_mainCharon.NetAddress) 
                ? null 
                : _bopProblems.FirstOrDefault(b => b.Ip == bopIp);
            if (bopProblem != null && (DateTime.Now - bopProblem.RebootStarted < _mikrotikRebootTimeout))
            {
                _rtuLog.AppendLine("Mikrotik is rebooting, step to the next port");
                return false;
            }

            var toggleResult = _mainCharon.SetExtendedActivePort(extendedPort.NetAddress, extendedPort.Port);
            switch (toggleResult)
            {
                case CharonOperationResult.Ok: return true;
                case CharonOperationResult.MainOtauError:
                    {
                        LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.ErrorTogglePort);
                        ClearArp();
                        var toggleRes = _mainCharon.SetExtendedActivePort(extendedPort.NetAddress, extendedPort.Port);
                        if (toggleRes != CharonOperationResult.Ok)
                        {
                            //TODO
                        }
                        return false;
                    }
                case CharonOperationResult.AdditionalOtauError:
                    {
                        bopProblem = _bopProblems.FirstOrDefault(b => b.Ip == bopIp);
                        if (bopProblem != null)
                        {
                            bopProblem.RebootStarted = DateTime.Now;
                            bopProblem.RebootAttempts++;
                            _rtuLog.AppendLine($"Reboot attempt N{bopProblem.RebootAttempts}");
                        }
                        else
                            _bopProblems.Add(new BopProblem(bopIp));

                        var mikrotik = new MikrotikInBop(_rtuLog, bopIp);
                        if (mikrotik.Connect())
                            mikrotik.Reboot();
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

    public class BopProblem
    {
        public string Ip { get; set; }
        public DateTime RebootStarted { get; set; }
        public int RebootAttempts { get; set; }

        public BopProblem(string ip)
        {
            Ip = ip;
            RebootStarted = DateTime.Now;
            RebootAttempts = 1;
        }
    }
}
