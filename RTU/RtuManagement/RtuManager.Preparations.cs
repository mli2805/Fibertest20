using System;
using System.Linq;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        private ReturnCode InitializeRtuManager(InitializeRtuDto dto)
        {
            // prohibit to send heartbeats
            ShouldSendHeartbeat.TryDequeue(out _);

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"RTU Manager version {_version}");

            var resetCharonResult = RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);
            if (resetCharonResult != ReturnCode.Ok)
                _rtuLog.AppendLine("Charon reset via COM port failed.");
            LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.Connecting);

            var otdrInitializationResult = InitializeOtdr();
            if (otdrInitializationResult != ReturnCode.Ok)
            {
                _rtuLog.AppendLine("OTDR initialization failed.");
                LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.ErrorConnectOtdr);
                return otdrInitializationResult;
            }

            var otauInitializationResult = dto != null
                ? ReInitializeOtauOnUsersRequest(dto)
                : InitializeOtau();
            if (otauInitializationResult != ReturnCode.Ok)
            {
                _rtuLog.AppendLine("OTAU initialization failed.");
                LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.ErrorConnectOtau);
                return otauInitializationResult;
            }

            _mainCharon.ShowOnDisplayMessageReady();

            _monitoringQueue = new MonitoringQueue(_rtuLog);
            _monitoringQueue.Load();
            GetMonitoringParams();

            _rtuLog.AppendLine("RTU Manager initialized successfully.");
            // permit to send heartbeats
            ShouldSendHeartbeat.Enqueue(new object());

            return ReturnCode.Ok;
        }

        private string _mfid;
        private string _mfsn;
        private string _omid;
        private string _omsn;
        private ReturnCode InitializeOtdr()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _rtuIni, _rtuLog);
            if (_otdrManager.LoadDll() != "")
                return ReturnCode.OtdrInitializationCannotLoadDll;

            if (!_otdrManager.InitializeLibrary())
                return ReturnCode.OtdrInitializationCannotInitializeDll;

            var otdrAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, DefaultIp);
            Thread.Sleep(300);
            if (!_otdrManager.ConnectOtdr(otdrAddress))
                return ReturnCode.FailedToConnectOtdr;

            _mfid = _otdrManager.InterOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetotdrinfoMfid);
            _rtuLog.AppendLine($"MFID = {_mfid}");
            _mfsn = _otdrManager.InterOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetotdrinfoMfsn);
            _rtuLog.AppendLine($"MFSN = {_mfsn}");
            _omid = _otdrManager.InterOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetotdrinfoOmid);
            _rtuLog.AppendLine($"OMID = {_omid}");
            _omsn = _otdrManager.InterOpWrapper.GetOtdrInfo(GetOtdrInfo.ServiceCmdGetotdrinfoOmsn);
            _rtuLog.AppendLine($"OMSN = {_omsn}");
            return ReturnCode.Ok;
            // return _otdrManager.ConnectOtdr(otdrAddress) ? ReturnCode.Ok : ReturnCode.FailedToConnectOtdr;
        }

        private ReturnCode ReInitializeOtauOnUsersRequest(InitializeRtuDto dto)
        {
            _rtuLog.AppendLine($"RTU hardware has {_mainCharon.Children.Count} additional OTAU ", messageLevel: 3);
            _rtuLog.AppendLine($"RTU in client has {dto.Children.Count} additional OTAU", messageLevel: 3);

            if (!_mainCharon.IsBopSupported)
                return dto.Children.Count > 0 ? ReturnCode.RtuDoesntSupportBop : ReturnCode.Ok;

            // detach bops if they are not attached in client
            foreach (var pair in _mainCharon.Children.ToArray())
            {
                _rtuLog.AppendLine($"RTU hardware has additional OTAU {pair.Value.Serial} on port {pair.Key}", messageLevel: 3);

                if (!dto.Children.ContainsKey(pair.Key) ||
                    pair.Value.Serial != dto.Children[pair.Key].Serial)
                {
                    _mainCharon.DetachOtauFromPort(pair.Key);
                }
            }

            // attach bops if they are attached in client (even if there are no such bop in reality)
            // initialize all child bops to get their states
            foreach (var pair in dto.Children)
            {
                _rtuLog.AppendLine($"RTU in client has additional OTAU {pair.Value.Serial} on port {pair.Key}", messageLevel: 3);

                if (!_mainCharon.Children.ContainsKey(pair.Key)
                    || _mainCharon.Children[pair.Key].Serial != pair.Value.Serial)
                {
                    _mainCharon.AttachOtauToPort(pair.Value.NetAddress, pair.Key);
                }
            }

            return InitializeOtau();
        }

        private ReturnCode InitializeOtau()
        {
            var otauIpAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtauIp, DefaultIp);
            _mainCharon = new Charon(new NetAddress(otauIpAddress, 23), _rtuIni, _rtuLog);
            var iniSize = _mainCharon.GetIniSize();
            _rtuLog.AppendLine($"Charon ini size is {iniSize}");
            _mainCharon.CharonIniSize = iniSize;
            var res = _mainCharon.InitializeOtauRecursively();
            if (res == _mainCharon.NetAddress)
                return ReturnCode.OtauInitializationError;

            var previousOwnPortCount = _rtuIni.Read(IniSection.RtuManager, IniKey.PreviousOwnPortCount, -1);
            if (previousOwnPortCount == -1)
                _rtuIni.Write(IniSection.RtuManager, IniKey.PreviousOwnPortCount, _mainCharon.OwnPortCount);
            if (previousOwnPortCount != _mainCharon.OwnPortCount)
            {
                if (_mainCharon.OwnPortCount != 1) // OTAU is changed - not broken
                {
                    _rtuIni.Write(IniSection.RtuManager, IniKey.PreviousOwnPortCount, _mainCharon.OwnPortCount);
                    _mainCharon.IsOk = true;
                }
                else
                {
                    _mainCharon.IsOk = false;
                }
            }

            if (res != null)
            {
                _rtuLog.AppendLine($"Child charon {res.ToStringA()} initialization failed.");
                _rtuLog.AppendLine("But RTU should work without BOP, so continue...");
            }
            return ReturnCode.Ok;
        }

        private void GetMonitoringParams()
        {
            _preciseMakeTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.PreciseMakeTimespan, 3600));
            _preciseSaveTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.PreciseSaveTimespan, 3600));
            _fastSaveTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.FastSaveTimespan, 3600));
        }

        private void DisconnectOtdr()
        {
            var otdrAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, DefaultIp);
            _otdrManager.DisconnectOtdr(otdrAddress);
            _rtuLog.AppendLine("Rtu is in MANUAL mode.");
        }

    }
}
