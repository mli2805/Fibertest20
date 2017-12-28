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

        public RtuWcfService(IMyLog serviceLog, RtuManager rtuManager)
        {
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
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

        public void BeginAttachOtau(AttachOtauDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (ShouldPerformOtauOperation())
                        _rtuManager.AttachOtau(dto, () => callbackChannel.EndAttachOtau(_rtuManager.OtauAttachedDto));
                    else
                        callbackChannel.EndAttachOtau(new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.RtuIsBusy });
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    var result = new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.RtuAttachOtauError, ErrorMessage = e.Message };
                    callbackChannel.EndAttachOtau(result);
                }
            });
        }

        public void BeginDetachOtau(DetachOtauDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (ShouldPerformOtauOperation())
                        _rtuManager.DetachOtau(dto, () => callbackChannel.EndDetachOtau(_rtuManager.OtauDetachedDto));
                    else
                        callbackChannel.EndDetachOtau(new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.RtuIsBusy });
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    var result = new OtauDetachedDto
                    {
                        IsDetached = true,
                        ReturnCode = ReturnCode.RtuDetachOtauError,
                        ErrorMessage = e.Message
                    };
                    callbackChannel.EndDetachOtau(result);
                }
            });
        }

        private bool ShouldPerformOtauOperation()
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User asks otau operation - Ignored - RTU is busy");
                return false;
            }
            if (_rtuManager.IsMonitoringOn)
            {
                _serviceLog.AppendLine("User asks otau operation - Ignored - RTU in AUTOMATIC mode");
                return false;
            }
            _serviceLog.AppendLine("User demands otau operation");
            return true;
        }

        public void BeginStopMonitoring(StopMonitoringDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (ShouldStop())
                        _rtuManager.StopMonitoring();
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                }
                _serviceLog.AppendLine($"StopMonitoring terminated successfully {!_rtuManager.IsMonitoringOn}");
                callbackChannel.EndStopMonitoring(!_rtuManager.IsMonitoringOn);
            });

        }

        private bool ShouldStop()
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User stops monitoring - Ignored - RTU is busy");
                return false;
            }
            if (!_rtuManager.IsMonitoringOn)
            {
                _serviceLog.AppendLine("User stops monitoring - Ignored - MANUAL mode already");
                return false;
            }
            _serviceLog.AppendLine("User demands stop monitoring");
            return true;
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
                    if (ShouldApplySettings())
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
                    result.ExceptionMessage = e.Message;
                    callbackChannel.EndApplyMonitoringSettings(result);
                }
            });
        }

        private bool ShouldApplySettings()
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User sent monitoring settings - Ignored - RTU is busy");
                return false;
            }
            _serviceLog.AppendLine("User sent monitoring settings - Accepted");
            return true;
        }

        public void BeginAssignBaseRef(AssignBaseRefsDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = new BaseRefAssignedDto();
                try
                {
                    result.ReturnCode = ShouldAssignBaseRef() ? _rtuManager.BaseRefsSaver.SaveBaseRefs(dto) : ReturnCode.RtuIsBusy;
                }
                catch (Exception e)
                {
                    _serviceLog.AppendLine("Thread pool: " + e);
                    result.ReturnCode = ReturnCode.RtuBaseRefAssignmentError;
                    result.ExceptionMessage = e.Message;
                }
                callbackChannel.EndAssignBaseRef(result);
            });
        }
        private bool ShouldAssignBaseRef()
        {
            if (!_rtuManager.IsRtuInitialized)
            {
                _serviceLog.AppendLine("User sent assign base refs command - Ignored - RTU is busy");
                return false;
            }
            _serviceLog.AppendLine("User sent assign base refs command - Accepted");
            return true;
        }

        public bool ToggleToPort(OtauPortDto dto)
        {
            if (!_rtuManager.IsRtuInitialized || _rtuManager.IsMonitoringOn)
            {
                _serviceLog.AppendLine("User demands port toggle - Ignored - RTU is busy");
                return false;
            }

            _serviceLog.AppendLine("User demands port toggle");
            _rtuManager.ToggleToPort(dto);
            return true;
        }

        public bool CheckLastSuccessfullMeasTime()
        {

            _serviceLog.AppendLine("WatchDog asks time of last successfull measurement");
            //            _rtuManager.
            return true;
        }
    }
}