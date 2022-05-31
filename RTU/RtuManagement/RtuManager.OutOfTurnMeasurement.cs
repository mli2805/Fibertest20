﻿using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        public void StartOutOfTurnMeasurement(DoOutOfTurnPreciseMeasurementDto dto, Action callback)
        {
            StopMonitoringWithRecovering("Out of turn precise measurement");

            callback?.Invoke();

            var moniResult = DoSecondMeasurement(new MonitorigPort(dto.PortWithTraceDto), false, BaseRefType.Precise, true);
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _rtuLog.AppendLine("Out of turn precise monitoring interrupted.");
                return;
            }

            var monitoringPort = _monitoringQueue.Queue.FirstOrDefault(p =>
                p.CharonSerial == dto.PortWithTraceDto.OtauPort.Serial
                && p.OpticalPort == dto.PortWithTraceDto.OtauPort.OpticalPort);

            if (monitoringPort != null)
            {
                monitoringPort.LastMoniResult = moniResult;
                monitoringPort.LastTraceState = moniResult.GetAggregatedResult();
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

        private void StopMonitoringWithRecovering(string customer)
        {
            _wasMonitoringOn = IsMonitoringOn;
            if (IsMonitoringOn)
                StopMonitoring(customer);

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"Start {customer}.");

            if (!_wasMonitoringOn)
            {
                var otdrAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, "192.168.88.101");
                var res = _otdrManager.ConnectOtdr(otdrAddress);
                if (!res)
                {
                    var recovery = RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                    // res = _otdrManager.ConnectOtdr(_mainCharon.NetAddress.Ip4Address);
                    // if (!res)
                    if (recovery != ReturnCode.Ok)
                        RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                }
            }
        }

    }
}
