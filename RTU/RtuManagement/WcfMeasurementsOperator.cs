﻿using System;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public class WcfMeasurementsOperator
    {
        private readonly IMyLog _serviceLog;
        private readonly RtuManager _rtuManager;

        public WcfMeasurementsOperator(IMyLog serviceLog, RtuManager rtuManager)
        {
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
        }

        public void ClientMeasurement(DoClientMeasurementDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ => {
                var result = new ClientMeasurementStartedDto();
                try
                {
                    result = _rtuManager.StartClientMeasurement(dto);
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    result.ReturnCode = ReturnCode.Error;
                    result.ExceptionMessage = e.Message;
                }
                callbackChannel.EndStartClientMeasurement(result);
            });
        }

        public void OutOfTurnPreciseMeasurement(DoOutOfTurnPreciseMeasurementDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ => {
                var result = new OutOfTurnMeasurementStartedDto();
                try
                {
                    result.ReturnCode = _rtuManager.StartOutOfTurnMeasurement(dto);
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    result.ReturnCode = ReturnCode.Error;
                    result.ExceptionMessage = e.Message;
                }
                callbackChannel.EndStartOutOfTurnMeasurement(result);
            });
        }


    }
}