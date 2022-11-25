using System;
using System.Globalization;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        public ClientMeasurementStartedDto ClientMeasurementStartedDto;

        public void DoClientMeasurement(DoClientMeasurementDto dto, Action callback)
        {
            if (!IsRtuInitialized)
            {
                _serviceLog.AppendLine("I am initializing now. Ignore command.");
                ClientMeasurementStartedDto = new ClientMeasurementStartedDto()
                { ClientMeasurementId = Guid.NewGuid(), ReturnCode = ReturnCode.RtuInitializationInProgress };
                callback?.Invoke();
                return;
            }

            if (IsAutoBaseMeasurementInProgress)
            {
                _serviceLog.AppendLine("Auto Base Measurement In Progress. Ignore command.");
                ClientMeasurementStartedDto = new ClientMeasurementStartedDto()
                { ClientMeasurementId = Guid.NewGuid(), ReturnCode = ReturnCode.RtuAutoBaseMeasurementInProgress };
                callback?.Invoke();
                return;
            }

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("DoClientMeasurement command received");

            if (!KeepOtdrConnection)
                StopMonitoringAndConnectOtdrWithRecovering(dto.IsForAutoBase ? "Auto base measurement" : "Measurement (Client)");

            KeepOtdrConnection = dto.KeepOtdrConnection;
            _rtuIni.Write(IniSection.Monitoring, IniKey.KeepOtdrConnection, KeepOtdrConnection);
            if (dto.IsForAutoBase)
            {
                IsAutoBaseMeasurementInProgress = true;
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsAutoBaseMeasurementInProgress, true);
                _rtuIni.Write(IniSection.Monitoring, IniKey.LastAutoBaseMeasurementTimestamp, DateTime.Now.ToString(CultureInfo.CurrentCulture));
            }

            ClientMeasurementStartedDto = new ClientMeasurementStartedDto()
            { ClientMeasurementId = Guid.NewGuid(), ReturnCode = ReturnCode.MeasurementClientStartedSuccessfully };
            callback?.Invoke(); // sends ClientMeasurementStartedDto (means "started successfully")

            var result = Measure(dto);
            _rtuLog.AppendLine(result.SorBytes != null
                ? $"Measurement Client done. Sor size is {result.SorBytes.Length}"
                : "Measurement (Client) failed");

            var _ = new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendClientMeasurementDone(result);
            if (dto.IsForAutoBase)
            {
                IsAutoBaseMeasurementInProgress = false;
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsAutoBaseMeasurementInProgress, false);
            }
            _rtuLog.EmptyLine();

            if (_wasMonitoringOn)
            {
                IsMonitoringOn = true;
                _wasMonitoringOn = false;
                RunMonitoringCycle();
            }
            else
            {
                if (!KeepOtdrConnection)
                    DisconnectOtdr();
            }
        }

        private ClientMeasurementResultDto Measure(DoClientMeasurementDto dto)
        {
            ClientMeasurementResultDto result = new ClientMeasurementResultDto().Initialize(dto);
            var toggleResult = ToggleToPort2(dto.OtauPortDto[0]);
            if (toggleResult != CharonOperationResult.Ok)
                return result.Set(dto.OtauPortDto[0],
                    toggleResult == CharonOperationResult.MainOtauError ? ReturnCode.RtuToggleToPortError : ReturnCode.RtuToggleToBopPortError);

            var prepareResult = dto.IsAutoLmax
                ? PrepareAutoLmaxMeasurement(dto)
                : PrepareClientMeasurement(dto);

            if (prepareResult != ReturnCode.Ok)
                return result.Set(dto.OtauPortDto[0], prepareResult);

            return ClientMeasurementItself(dto, dto.OtauPortDto[0]);
        }

        private ReturnCode PrepareAutoLmaxMeasurement(DoClientMeasurementDto dto)
        {
            var lmax = _otdrManager.InterOpWrapper.GetLinkCharacteristics(out ConnectionParams cp);
            if (lmax.Equals(-1.0))
            {
                _rtuLog.AppendLine("Failed to get link characteristics");
                return ReturnCode.MeasurementPreparationError;
            }

            if (lmax.Equals(0) && cp.splice == 0)
            {
                if (RunMainCharonRecovery() != ReturnCode.Ok)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                return ReturnCode.MeasurementPreparationError;
            }

            if (cp.snr_almax == 0)
            {
                _rtuLog.AppendLine("SNR == 0, No fiber!");
                return ReturnCode.SnrIs0;
            }
            var values = AutoBaseParams.GetPredefinedParamsForLmax(lmax, "IIT MAK-100");
            if (values == null)
            {
                _rtuLog.AppendLine("Lmax is out of valid range");
                return ReturnCode.InvalidValueOfLmax;
            }

            var positions = _otdrManager.InterOpWrapper.ValuesToPositions(dto.SelectedMeasParams, values, _treeOfAcceptableMeasParams);
            if (!_otdrManager.InterOpWrapper.SetMeasParamsByPosition(positions))
            {
                _rtuLog.AppendLine("Failed to set measurement parameters");
                return ReturnCode.MeasurementPreparationError;
            }
            _rtuLog.AppendLine("Auto measurement parameters applied");
            return ReturnCode.Ok;
        }

        private ReturnCode PrepareClientMeasurement(DoClientMeasurementDto dto)
        {
            if (!_otdrManager.InterOpWrapper.SetMeasParamsByPosition(dto.SelectedMeasParams))
            {
                _rtuLog.AppendLine("Failed to set measurement parameters");
                return ReturnCode.MeasurementPreparationError;
            }
            _rtuLog.AppendLine("User's measurement parameters applied");
            return ReturnCode.Ok;
        }

        private ClientMeasurementResultDto ClientMeasurementItself(DoClientMeasurementDto dto, OtauPortDto currentOtauPortDto)
        {
            var result = new ClientMeasurementResultDto().Initialize(dto);
            var activeBop = currentOtauPortDto.IsPortOnMainCharon
                ? null
                : new Charon(new NetAddress(currentOtauPortDto.NetAddress.Ip4Address, TcpPorts.IitBop), false, _rtuIni, _rtuLog);
            _cancellationTokenSource = new CancellationTokenSource();
            var measResult = _otdrManager.DoManualMeasurement(_cancellationTokenSource, true, activeBop);

            // во время измерения или прямо сейчас
            if (measResult == ReturnCode.MeasurementInterrupted || _cancellationTokenSource.IsCancellationRequested)
            {
                _rtuLog.AppendLine("Measurement (Client) interrupted.");
                _wasMonitoringOn = false;
                KeepOtdrConnection = false;
                _rtuIni.Write(IniSection.Monitoring, IniKey.KeepOtdrConnection, KeepOtdrConnection);
                return result.Set(currentOtauPortDto, ReturnCode.MeasurementInterrupted);
            }

            if (measResult != ReturnCode.MeasurementEndedNormally)
            {
                if (RunMainCharonRecovery() != ReturnCode.Ok)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                return result.Set(currentOtauPortDto, ReturnCode.MeasurementError);
            }

            var lastSorDataBuffer = _otdrManager.GetLastSorDataBuffer();
            if (lastSorDataBuffer == null)
                return result.Set(currentOtauPortDto, ReturnCode.MeasurementError);

            return result.Set(currentOtauPortDto, ReturnCode.MeasurementEndedNormally,
               !dto.IsForAutoBase
                    ? _otdrManager.ApplyAutoAnalysis(lastSorDataBuffer)
                    : dto.IsInsertNewEvents 
                        ? _otdrManager.Sf780_779(lastSorDataBuffer)
                        : _otdrManager.Sf780(lastSorDataBuffer));
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
