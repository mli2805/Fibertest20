﻿using System;
using System.IO;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuManagement
{

    public partial class RtuManager
    {
        public ClientMeasurementStartedDto ClientMeasurementStartedDto;

        public void DoClientMeasurement(DoClientMeasurementDto dto, Action callback)
        {
            StopMonitoringAndConnectOtdrWithRecovering(dto.IsForAutoBase ? "Auto base measurement" : "Measurement (Client)");
            if (dto.IsForWholeRtu)
            {
                IsRtuAutoBaseMode = true;
                SaveDoClientMeasurementDto(dto);
            }

            ClientMeasurementStartedDto = new ClientMeasurementStartedDto() { ClientMeasurementId = Guid.NewGuid(), ReturnCode = ReturnCode.Ok };
            callback?.Invoke(); // sends ClientMeasurementStartedDto (means "started successfully")

            Measure(dto);

            IsRtuAutoBaseMode = false;

            if (_wasMonitoringOn)
            {
                IsMonitoringOn = true;
                _wasMonitoringOn = false;
                RunMonitoringCycle();
            }
            else
                DisconnectOtdr();
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };
        private readonly string _autoBaseDtoFile = Utils.FileNameForSure(@"..\ini\", @"autobase.dto", false);

        public void SaveDoClientMeasurementDto(DoClientMeasurementDto dto)
        {
            try
            {
                var json = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
                File.WriteAllText(_autoBaseDtoFile, json);
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"AutoBase dto saving: {e.Message}");
            }
        }

        public DoClientMeasurementDto LoadDoClientMeasurementDto()
        {
            DoClientMeasurementDto result = null;
            try
            {
                var context = File.ReadAllText(_autoBaseDtoFile);
                result = JsonConvert.DeserializeObject<DoClientMeasurementDto>(context);
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"AutoBase dto loading: {e.Message}");
            }

            return result;
        }

        private void Measure(DoClientMeasurementDto dto)
        {
            foreach (var listOfOtauPort in dto.OtauPortDtoList)
            {
                if (ToggleToPort(listOfOtauPort[0]))
                {
                    var prepareResult = dto.IsAutoLmax
                        ? PrepareAutoLmaxMeasurement(dto)
                        : PrepareClientMeasurement(dto);

                    ClientMeasurementResultDto result;
                    if (prepareResult)
                    {
                        result = ClientMeasurementItself(dto, listOfOtauPort[0]);
                        if (result == null) // measurement interrupted
                        {
                            _wasMonitoringOn = false;
                            break;
                        }
                    }
                    else
                    {
                        result = new ClientMeasurementResultDto()
                        {
                            ReturnCode = ReturnCode.InvalidValueOfLmax,
                            ConnectionId = dto.ConnectionId,
                            ClientIp = dto.ClientIp,
                            OtauPortDto = listOfOtauPort[0],
                        };
                    }

                    var sendResult = new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog)
                        .SendClientMeasurementDone(result);
                    _rtuLog.AppendLine(sendResult
                        ? $"Send client measurement result ({result.ReturnCode})"
                        : $"Can't send client measurement result to {_serverAddresses.Main.ToStringA()}");
                    _rtuLog.EmptyLine();
                }
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
                return new ClientMeasurementResultDto()
                {
                    ClientMeasurementId = Guid.NewGuid(),
                    ReturnCode = ReturnCode.MeasurementError,
                    ConnectionId = dto.ConnectionId,
                    ClientIp = dto.ClientIp,
                    OtauPortDto = currentOtauPortDto,
                };
            return new ClientMeasurementResultDto()
            {
                ClientMeasurementId = Guid.NewGuid(),
                ReturnCode = ReturnCode.MeasurementEndedNormally,
                ConnectionId = dto.ConnectionId,
                ClientIp = dto.ClientIp,
                OtauPortDto = currentOtauPortDto,
                SorBytes = dto.IsForAutoBase
                    ? _otdrManager.Sf780_779(lastSorDataBuffer)
                    : _otdrManager.ApplyAutoAnalysis(lastSorDataBuffer),
            };
        }
    }
}
