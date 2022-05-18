using System;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
{

    public partial class RtuManager
    {
        public ClientMeasurementStartedDto ClientMeasurementStartedDto;

        public void DoClientMeasurement(DoClientMeasurementDto dto, Action callback)
        {
            _wasMonitoringOn = IsMonitoringOn;
            ClientMeasurementStartedDto = new ClientMeasurementStartedDto() { ClientMeasurementId = Guid.NewGuid(), OtauPortDto = dto.OtauPortDtoList[0] };

            if (dto.IsForAutoBase && dto.IsAutoLmax)
                PrepareAutoLmaxMeasurement(dto);
            else
                PrepareClientMeasurement(dto);
            callback?.Invoke(); // send "started"

            var result = ClientMeasurementItself(dto);
            if (result == null)
            {
                _wasMonitoringOn = false;
                return;
            }

            var sendResult = new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendClientMeasurementDone(result);
            _serviceLog.AppendLine(sendResult
                ? "RTU sent client measurement result"
                : $"Can't send client measurement result to {_serverAddresses.Main.ToStringA()}");


            if (_wasMonitoringOn)
            {
                IsMonitoringOn = true;
                _wasMonitoringOn = false;
                RunMonitoringCycle();
            }
            else
                DisconnectOtdr();
        }

        private void PrepareAutoLmaxMeasurement(DoClientMeasurementDto dto)
        {
            StopMoniAndTogglePort(dto);
            if (ClientMeasurementStartedDto.ReturnCode == ReturnCode.RtuToggleToPortError)
                return;

            var lmax = _otdrManager.InterOpWrapper.GetLinkCharacteristics();
            var values = AutoBaseParams.GetPredefinedParamsForLmax(lmax, "IIT MAK-100");
            if (values == null)
            {
                ClientMeasurementStartedDto.ReturnCode = ReturnCode.TooLongForAutoBase;
                return;
            }

            var positions = _otdrManager.InterOpWrapper.ValuesToPositions(dto.SelectedMeasParams, values, _treeOfAcceptableMeasParams);

            _otdrManager.InterOpWrapper.SetMeasParamsByPosition(positions);
            _rtuLog.AppendLine("Auto measurement parameters applied");
        }

        private void PrepareClientMeasurement(DoClientMeasurementDto dto)
        {
            StopMoniAndTogglePort(dto);
            if (ClientMeasurementStartedDto.ReturnCode == ReturnCode.RtuToggleToPortError)
                return;

            _otdrManager.InterOpWrapper.SetMeasParamsByPosition(dto.SelectedMeasParams);
            _rtuLog.AppendLine("User's measurement parameters applied");
        }

        private void StopMoniAndTogglePort(DoClientMeasurementDto dto)
        {
            if (IsMonitoringOn)
                StopMonitoring("Measurement (Client)");

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start Measurement (Client).");

            if (!_wasMonitoringOn)
            {
                var otdrAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, "192.168.88.101");
                var res = _otdrManager.ConnectOtdr(otdrAddress);
                if (!res)
                {
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                    res = _otdrManager.ConnectOtdr(_mainCharon.NetAddress.Ip4Address);
                    if (!res)
                        RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                }
            }

            ClientMeasurementStartedDto.ReturnCode = !ToggleToPort(dto.OtauPortDtoList[0]) ? ReturnCode.RtuToggleToPortError : ReturnCode.Ok;
        }

        private ClientMeasurementResultDto ClientMeasurementItself(DoClientMeasurementDto dto)
        {
            var activeBop = dto.OtauPortDtoList[0].IsPortOnMainCharon
                ? null
                : new Charon(new NetAddress(dto.OtauIp, dto.OtauTcpPort), false, _rtuIni, _rtuLog);
            _cancellationTokenSource = new CancellationTokenSource();
            _otdrManager.DoManualMeasurement(_cancellationTokenSource, true, activeBop);
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _rtuLog.AppendLine("Measurement (Client) interrupted.");
                return null;
            }
            var lastSorDataBuffer = _otdrManager.GetLastSorDataBuffer();
            if (lastSorDataBuffer == null)
                return new ClientMeasurementResultDto()
                {
                    ReturnCode = ReturnCode.MeasurementError,
                    ConnectionId = dto.ConnectionId,
                    ClientIp = dto.ClientIp,
                };
            return new ClientMeasurementResultDto()
            {
                ReturnCode = ReturnCode.MeasurementEndedNormally,
                ConnectionId = dto.ConnectionId,
                ClientIp = dto.ClientIp,
                SorBytes = dto.IsForAutoBase
                    ? _otdrManager.Sf780_779(lastSorDataBuffer)
                    : _otdrManager.ApplyAutoAnalysis(lastSorDataBuffer),
            };
        }


    }
}
