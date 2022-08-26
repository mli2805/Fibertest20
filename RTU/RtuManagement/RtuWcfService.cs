using System;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RtuWcfService : IRtuWcfService
    {
        private readonly IMyLog _serviceLog;
        private readonly RtuManager _rtuManager;
        private readonly WcfOtauOperator _wcfOtauOperator;
        private readonly WcfMeasurementsOperator _wcfMeasurementsOperator;
        private readonly RtuWcfOperationsPermissions _rtuWcfOperationsPermissions;

        private readonly BaseRefsSaver _baseRefsSaver;

        public RtuWcfService(IMyLog serviceLog, RtuManager rtuManager, BaseRefsSaver baseRefsSaver,
            WcfOtauOperator wcfOtauOperator, WcfMeasurementsOperator wcfMeasurementsOperator,
            RtuWcfOperationsPermissions rtuWcfOperationsPermissions)
        {
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
            _wcfOtauOperator = wcfOtauOperator;
            _wcfMeasurementsOperator = wcfMeasurementsOperator;
            _rtuWcfOperationsPermissions = rtuWcfOperationsPermissions;

            _baseRefsSaver = baseRefsSaver;
        }

        public void BeginInitialize(InitializeRtuDto dto)
        {
            _serviceLog.AppendLine("User demands initialization - OK");
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    _rtuManager.Initialize(dto, () => callbackChannel.EndInitialize(_rtuManager.GetInitializationResult()));
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                }
            });
        }

        public void BeginStopMonitoring(StopMonitoringDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (_rtuWcfOperationsPermissions.ShouldStop())
                        _rtuManager.StopMonitoringRequest("Stop monitoring");
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                }
                _serviceLog.AppendLine($"StopMonitoring terminated successfully {!_rtuManager.IsMonitoringOn}");
                callbackChannel.EndStopMonitoring(!_rtuManager.IsMonitoringOn);
            });
        }

        public void BeginApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            _serviceLog.AppendLine("User sent monitoring settings");
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.MonitoringSettingsAppliedSuccessfully };
                try
                {
                    if (_rtuWcfOperationsPermissions.ShouldExecute("User sent monitoring settings"))
                        _rtuManager.ChangeSettings(dto, () => callbackChannel.EndApplyMonitoringSettings(result));
                    else
                    {
                        result.ReturnCode = ReturnCode.RtuIsBusy;
                        callbackChannel.EndApplyMonitoringSettings(result);
                    }
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    result.ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError;
                    result.ErrorMessage = e.Message;
                    callbackChannel.EndApplyMonitoringSettings(result);
                }
            });
        }

        public void BeginAssignBaseRef(AssignBaseRefsDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = new BaseRefAssignedDto();
                try
                {
                    _serviceLog.EmptyLine();
                    result.ReturnCode = _rtuWcfOperationsPermissions.ShouldExecute("User sent assign base refs command")
                        ? _baseRefsSaver.SaveBaseRefs(dto)
                        : ReturnCode.RtuIsBusy;
                    _serviceLog.EmptyLine();
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    result.ReturnCode = ReturnCode.RtuBaseRefAssignmentError;
                    result.ErrorMessage = e.Message;
                }
                callbackChannel.EndAssignBaseRef(result);
            });
        }

        public void BeginAttachOtau(AttachOtauDto dto) { _wcfOtauOperator.AttachOtau(dto); }

        public void BeginDetachOtau(DetachOtauDto dto) { _wcfOtauOperator.DetachOtau(dto); }

        public void BeginClientMeasurement(DoClientMeasurementDto dto)
        {
            _serviceLog.EmptyLine();
            _serviceLog.AppendLine("Client asks to do measurement(client)");
            if (_rtuManager.IsRtuInitialized)
                _wcfMeasurementsOperator.StartClientMeasurement(dto);
            else
            {
                _serviceLog.AppendLine("RTU initialization is in progress.");
                var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
                callbackChannel.EndStartClientMeasurement(new ClientMeasurementStartedDto(){ReturnCode = ReturnCode.RtuInitializationInProgress});
            }
        }

        public void BeginOutOfTurnPreciseMeasurement(DoOutOfTurnPreciseMeasurementDto dto) { _wcfMeasurementsOperator.OutOfTurnPreciseMeasurement(dto); }
        public void BeginInterruptMeasurement(InterruptMeasurementDto dto)
        {
            _serviceLog.EmptyLine();
            _serviceLog.AppendLine("Client asks to interrupt current measurement");
            _wcfMeasurementsOperator.InterruptMeasurement(dto);
        }

        public bool CheckLastSuccessfulMeasTime()
        {
            _serviceLog.AppendLine("WatchDog asks time of last successful measurement");
            return true;
        }
    }
}