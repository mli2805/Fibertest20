using System;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public class WcfOtauOperator
    {
        private readonly IMyLog _serviceLog;
        private readonly RtuManager _rtuManager;
        private readonly RtuWcfOperationsPermissions _rtuWcfOperationsPermissions;

        public WcfOtauOperator(IMyLog serviceLog, RtuManager rtuManager, RtuWcfOperationsPermissions rtuWcfOperationsPermissions)
        {
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
            _rtuWcfOperationsPermissions = rtuWcfOperationsPermissions;
        }

        public void AttachOtau(AttachOtauDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ => {
                try
                {
                    if (_rtuWcfOperationsPermissions.ShouldPerformOtauOperation())
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

        public void DetachOtau(DetachOtauDto dto)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRtuWcfServiceBackward>();
            ThreadPool.QueueUserWorkItem(_ => {
                try
                {
                    if (_rtuWcfOperationsPermissions.ShouldPerformOtauOperation())
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

    }
}