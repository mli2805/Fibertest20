using System;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        public ClientMeasurementStartedDto ClientMeasurementStartedDto;

        public void DoClientMeasurement(DoClientMeasurementDto dto, Action callback)
        {
            StopMonitoringAndConnectOtdrWithRecovering(dto.IsForAutoBase ? "Auto base measurement" : "Measurement (Client)");

            ClientMeasurementStartedDto = new ClientMeasurementStartedDto() { ClientMeasurementId = Guid.NewGuid(), ReturnCode = ReturnCode.Ok };
            callback?.Invoke(); // sends ClientMeasurementStartedDto (means "started successfully")

            if (dto.IsForWholeRtu)
                DoAutoBaseMeasurementsForRtu(dto);
            else
                Measure(dto);

            if (_wasMonitoringOn)
            {
                IsMonitoringOn = true;
                _wasMonitoringOn = false;
                RunMonitoringCycle();
            }
            else
                DisconnectOtdr();
        }

        private void Measure(DoClientMeasurementDto dto)
        {
            if (ToggleToPort(dto.OtauPortDtoList[0][0]))
            {
                var prepareResult = dto.IsAutoLmax
                    ? PrepareAutoLmaxMeasurement(dto)
                    : PrepareClientMeasurement(dto);

                ClientMeasurementResultDto result;
                if (prepareResult)
                {
                    result = ClientMeasurementItself(dto, dto.OtauPortDtoList[0][0]);
                    if (result == null) // measurement interrupted
                    {
                        _wasMonitoringOn = false;
                    }
                }
                else
                {
                    result = new ClientMeasurementResultDto()
                    {
                        ReturnCode = ReturnCode.InvalidValueOfLmax,
                        ConnectionId = dto.ConnectionId,
                        ClientIp = dto.ClientIp,
                        OtauPortDto = dto.OtauPortDtoList[0][0],
                    };
                }

                var _ = new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog)
                    .SendClientMeasurementDone(result);
                _rtuLog.EmptyLine();
            }
        }

        private bool PrepareAutoLmaxMeasurement(DoClientMeasurementDto dto)
        {
            var lmax = _otdrManager.InterOpWrapper.GetLinkCharacteristics();
            var values = AutoBaseParams.GetPredefinedParamsForLmax(lmax, "IIT MAK-100");
            if (values == null)
            {
                _rtuLog.AppendLine("Failed to choose measurement parameters");
                return false;
            }

            var positions = _otdrManager.InterOpWrapper.ValuesToPositions(dto.SelectedMeasParams, values, _treeOfAcceptableMeasParams);
            _otdrManager.InterOpWrapper.SetMeasParamsByPosition(positions);
            _rtuLog.AppendLine("Auto measurement parameters applied");
            return true;
        }

        private bool PrepareClientMeasurement(DoClientMeasurementDto dto)
        {
            _otdrManager.InterOpWrapper.SetMeasParamsByPosition(dto.SelectedMeasParams);
            _rtuLog.AppendLine("User's measurement parameters applied");
            return true;
        }

        private ClientMeasurementResultDto ClientMeasurementItself(DoClientMeasurementDto dto, OtauPortDto currentOtauPortDto)
        {
            var result = new ClientMeasurementResultDto().Initialize(dto);
            var activeBop = currentOtauPortDto.IsPortOnMainCharon
                ? null
                : new Charon(new NetAddress(currentOtauPortDto.NetAddress.Ip4Address, TcpPorts.IitBop), false, _rtuIni, _rtuLog);
            _cancellationTokenSource = new CancellationTokenSource();
            _otdrManager.DoManualMeasurement(_cancellationTokenSource, true, activeBop);
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _rtuLog.AppendLine("Measurement (Client) interrupted.");
                return null;
            }
            var lastSorDataBuffer = _otdrManager.GetLastSorDataBuffer();
            if (lastSorDataBuffer == null)
                return result.Set(currentOtauPortDto, ReturnCode.MeasurementError);

            return result.Set(currentOtauPortDto, ReturnCode.MeasurementEndedNormally,
               dto.IsForAutoBase
                    ? _otdrManager.Sf780_779(lastSorDataBuffer)
                    : _otdrManager.ApplyAutoAnalysis(lastSorDataBuffer));
        }
    }

    public static class ClientMeasurementResultFactory
    {
        public static ClientMeasurementResultDto Initialize(this ClientMeasurementResultDto result, DoClientMeasurementDto dto)
        {
            result.ConnectionId = dto.ConnectionId;
            result.ClientIp = dto.ClientIp;

            return result;
        }

        public static ClientMeasurementResultDto Set(this ClientMeasurementResultDto result,
            OtauPortDto otauPortDto, ReturnCode returnCode, byte[] sorBytes = null)
        {
            result.ClientMeasurementId = Guid.NewGuid();
            result.ReturnCode = returnCode;
            result.OtauPortDto = otauPortDto;
            result.SorBytes = sorBytes;

            return result;
        }
    }
}
