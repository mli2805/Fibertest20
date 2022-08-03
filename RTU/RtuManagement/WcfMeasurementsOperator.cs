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
                            new RequestAnswer()
                            {
                                ReturnCode = ReturnCode.Ok, ErrorMessage = "Out of turn measurement started(!) successfully."
                            }));
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    var result = new RequestAnswer { ReturnCode = ReturnCode.Error, ErrorMessage = e.Message };
                    callbackChannel.EndStartOutOfTurnMeasurement(result);
                }
            });
        }  
        
        public void InterruptMeasurement(InterruptMeasurementDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    _rtuManager.InterruptMeasurement(dto, () => callbackChannel.EndInterruptMeasurement(
                            new RequestAnswer()
                            {
                                ReturnCode = ReturnCode.Ok, ErrorMessage = "Measurement interrupted successfully."
                            }));
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    var result = new RequestAnswer { ReturnCode = ReturnCode.Error, ErrorMessage = e.Message };
                    callbackChannel.EndInterruptMeasurement(result);
                }
            });
        }
    }
}