using System;
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

        public void StartClientMeasurement(DoClientMeasurementDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    _rtuManager.DoClientMeasurement(dto, () => callbackChannel.EndStartClientMeasurement(_rtuManager.ClientMeasurementStartedDto));
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    var result = new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.Error, ErrorMessage = e.Message };
                    callbackChannel.EndStartClientMeasurement(result);
                }
            });
        }

        public void OutOfTurnPreciseMeasurement(DoOutOfTurnPreciseMeasurementDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    _rtuManager.StartOutOfTurnMeasurement(dto, () => callbackChannel.EndStartOutOfTurnMeasurement(
                            new OutOfTurnMeasurementStartedDto() { ReturnCode = ReturnCode.Ok, ExceptionMessage = "just for test purposes" }));
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    var result = new OutOfTurnMeasurementStartedDto { ReturnCode = ReturnCode.Error, ExceptionMessage = e.Message };
                    callbackChannel.EndStartOutOfTurnMeasurement(result);
                }
            });
        }
    }
}