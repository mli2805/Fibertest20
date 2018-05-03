using System;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        public ClientMeasurementStartedDto ClientMeasurementStartedDto = new ClientMeasurementStartedDto();

        public void DoClientMeasurement(DoClientMeasurementDto dto, Action callback)
        {
            _wasMonitoringOn = IsMonitoringOn;
            PrepareClientMeasurement(dto);
            callback?.Invoke(); // send "started"

            var result = ClientMeasurementItself(dto);
            if (result == null)
                return;

            new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendClientMeasurementDone(result);
            if (_wasMonitoringOn)
            {
                IsMonitoringOn = true;
                RunMonitoringCycle();
            }
            else
                DisconnectOtdr();
        }

        private void PrepareClientMeasurement(DoClientMeasurementDto dto)
        {
            if (IsMonitoringOn)
            {
                StopMonitoring("Measurement (Client)");
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start Measurement (Client).");

            var res = _otdrManager.ConnectOtdr(_mainCharon.NetAddress.Ip4Address);
            if (!res)
            {
                RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                res = _otdrManager.ConnectOtdr(_mainCharon.NetAddress.Ip4Address);
                if (!res)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            }

            _otdrManager.InterOpWrapper.SetMeasurementParametersFromUserInput(dto.SelectedMeasParams);
            _rtuLog.AppendLine("User's measurement parameters applied");

            ClientMeasurementStartedDto.ReturnCode = !ToggleToPort(dto.OtauPortDto) ? ReturnCode.RtuToggleToPortError : ReturnCode.Ok;
        }

        private ClientMeasurementDoneDto ClientMeasurementItself(DoClientMeasurementDto dto)
        {
            var activeBop = dto.OtauPortDto.IsPortOnMainCharon
                ? null
                : new Charon(new NetAddress(dto.OtauPortDto.OtauIp, dto.OtauPortDto.OtauTcpPort), _rtuIni, _rtuLog);
            _cancellationTokenSource = new CancellationTokenSource();
            _otdrManager.DoManualMeasurement(_cancellationTokenSource, true, activeBop);
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _rtuLog.AppendLine("Measurement (Client) interrupted.");
                return null;
            }
            var lastSorDataBuffer = _otdrManager.GetLastSorDataBuffer();
            if (lastSorDataBuffer == null)
                return new ClientMeasurementDoneDto()
                {
                    ReturnCode = ReturnCode.MeasurementError,
                    ClientId = dto.ClientId,
                };
            return new ClientMeasurementDoneDto()
            {
                ReturnCode = ReturnCode.Ok,
                ClientId = dto.ClientId,
                SorBytes = _otdrManager.ApplyAutoAnalysis(lastSorDataBuffer),
            };
        }

        public void StartOutOfTurnMeasurement(DoOutOfTurnPreciseMeasurementDto dto, Action callback)
        {
            _wasMonitoringOn = IsMonitoringOn;
            if (IsMonitoringOn)
            {
                StopMonitoring("Out of turn monitoring.");
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start out of turn precise monitoring.");

            var res = _otdrManager.ConnectOtdr(_mainCharon.NetAddress.Ip4Address);
            if (!res)
            {
                RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                res = _otdrManager.ConnectOtdr(_mainCharon.NetAddress.Ip4Address);
                if (!res)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            }

            callback?.Invoke();

            DoSecondMeasurement(new MonitorigPort(dto.PortWithTraceDto), false, BaseRefType.Precise, true);
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _rtuLog.AppendLine("Out of turn precise monitoring interrupted.");
                return;
            }

            if (_wasMonitoringOn)
            {
                IsMonitoringOn = true;
                _wasMonitoringOn = false;
                RunMonitoringCycle();
            }
            else
                DisconnectOtdr();
        }

    }
}
