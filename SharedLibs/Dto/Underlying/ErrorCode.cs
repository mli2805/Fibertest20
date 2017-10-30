using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Dto
{
    public enum ErrorCode
    {
        Ok = 1,
        Error = 0,

        RtuInitializationError = 1000,
        OtdrInitializationCannotLoadDll = 1001,
        OtdrInitializationCannotInitializeDll = 1002,

        MeasurementError = 1100,
        MeasurementEndedNormally = 1101,
        MeasurementPreparationError = 1102,
        MeasurementInterrupted = 1103,

        TcpConnectionError = 200,
        C2DWcfConnectionError = 201,
        C2DWcfOperationError = 202,

    }

    public static class ErrorCodeExt
    {
        public static string GetLocalizedString(this ErrorCode errorCode, string exceptionMessage)
        {
            switch (errorCode)
            {
                case ErrorCode.Error:
                    return Resources.SID_Error_;
                case ErrorCode.OtdrInitializationCannotLoadDll:
                    return Resources.SID_Cannot_find_dll_file_;
                case ErrorCode.OtdrInitializationCannotInitializeDll:
                    return Resources.SID_Cannot_initialize_dll_;
                case ErrorCode.C2DWcfConnectionError:
                    return Resources.SID_Cannot_establish_connection_with_DataCenter_;
                case ErrorCode.C2DWcfOperationError:
                    return Resources.SID_RTU_initialization_error_ + $"\n\n{exceptionMessage}";
                default: return "";
            }
        }
    }
}
