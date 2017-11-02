using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Dto
{
    public enum ReturnCode
    {
        Error = 0,
        Ok = 1,

        RtuInitializationError = 1000,
        OtderInitializedSuccessfully = 1001,
        OtdrInitializationCannotLoadDll = 1002,
        OtdrInitializationCannotInitializeDll = 1003,

        MeasurementError = 1100,
        MeasurementEndedNormally = 1101,
        MeasurementPreparationError = 1102,
        MeasurementInterrupted = 1103,

        TcpConnectionError = 2000,
        C2DWcfConnectionError = 2001,
        C2DWcfOperationError = 2002,

        DbError = 3000,
        DbInitializedSuccessfully = 3001,
        DbIsNotInitializedError = 3002,

        ClientRegistrationError = 9000,
        ClientRegisteredSuccessfully = 9001,
        NoSuchUserOrWrongPassword = 9002,
        ThisUserRegisteredOnAnotherPc = 9003,
    }

    public static class ErrorCodeExt
    {
        public static string GetLocalizedString(this ReturnCode returnCode, string exceptionMessage = "")
        {
            switch (returnCode)
            {
                case ReturnCode.Error:
                    return Resources.SID_Error_;
                case ReturnCode.OtdrInitializationCannotLoadDll:
                    return Resources.SID_Cannot_find_dll_file_;
                case ReturnCode.OtdrInitializationCannotInitializeDll:
                    return Resources.SID_Cannot_initialize_dll_;

                    // 2000
                case ReturnCode.C2DWcfConnectionError:
                    return Resources.SID_Cannot_establish_connection_with_DataCenter_;
                case ReturnCode.C2DWcfOperationError:
                    return Resources.SID_RTU_initialization_error_ + $"\n\n{exceptionMessage}";

                    // 9000
                case ReturnCode.NoSuchUserOrWrongPassword:
                    return "No such user or wrong password!";
                case ReturnCode.ThisUserRegisteredOnAnotherPc:
                    return "User with the same name is registered on another PC";
                default: return "";
            }
        }
    }
}
