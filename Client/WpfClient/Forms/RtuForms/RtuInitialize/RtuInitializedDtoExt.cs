using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.RtuOccupy;

namespace Iit.Fibertest.Client
{
    public static class RtuInitializedDtoExt
    {
        public static MyMessageBoxViewModel CreateMessageBox(this RtuInitializedDto dto, string rtuTitle)
        {
            switch (dto.ReturnCode)
            {
                case ReturnCode.Ok:
                case ReturnCode.RtuInitializedSuccessfully:
                    var msg = dto.Children.Any(c => !c.Value.IsOk)
                        ? Resources.SID_RTU_initialized2
                        : Resources.SID_RTU_initialized_successfully_;
                    return new MyMessageBoxViewModel(MessageType.Information, msg);
                case ReturnCode.RtuDoesNotSupportBop:
                case ReturnCode.RtuTooBigPortNumber:
                    var strs = new List<string>()
                    {
                        dto.ReturnCode.GetLocalizedString(), "", Resources.SID_Detach_BOP_manually, 
                        Resources.SID_and_start_initialization_again_
                    };
                    return new MyMessageBoxViewModel(MessageType.Error, strs);
                case ReturnCode.RtuIsBusy:
                case ReturnCode.RtuInitializationInProgress:
                case ReturnCode.RtuAutoBaseMeasurementInProgress:
                    return new MyMessageBoxViewModel(
                    MessageType.Error, new List<string>()
                        {
                            string.Format(Resources.SID_RTU__0__is_busy_, rtuTitle), "", dto.RtuOccupationState.GetLocalized(),
                        }, 1);
                case ReturnCode.RtuInitializationError:
                    return new MyMessageBoxViewModel(MessageType.Error, 
                        dto.ReturnCode.GetLocalizedWithOsInfo(dto.ErrorMessage).Split('\n'), 0);
                case ReturnCode.OtauInitializationError:
                case ReturnCode.OtdrInitializationFailed:
                case ReturnCode.FailedToConnectOtdr:
                default:
                    var strs1 = new List<string>()
                    {
                        ReturnCode.RtuInitializationError.GetLocalizedString(), "", 
                    };
                    strs1.AddRange(dto.ReturnCode.GetLocalizedWithOsInfo(dto.ErrorMessage).Split('\n'));
                    return new MyMessageBoxViewModel(MessageType.Error, strs1, 0);
            }
        }

        public static string CreateLogMessage(this RtuInitializedDto dto)
        {
            var rtuName = dto.RtuAddresses != null ? $@"RTU {dto.RtuAddresses.Main.Ip4Address}" : @"RTU";
            var message = dto.IsInitialized
                ? $@"{rtuName} initialized successfully."
                : $@"{rtuName} initialization failed. " + Environment.NewLine + dto.ReturnCode.GetLocalizedString();
            if (!string.IsNullOrEmpty(dto.ErrorMessage))
                message += Environment.NewLine + dto.ErrorMessage;
            return message;
        }
    }
}
