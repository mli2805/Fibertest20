using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Dto
{
    public enum ReturnCode
    {
        Error = 0,
        Ok = 1,

        RtuInitializationError = 1000,
        OtdrInitializedSuccessfully = 1001,
        OtdrInitializationCannotLoadDll = 1002,
        OtdrInitializationCannotInitializeDll = 1003,
        RtuIsBusy = 1004,
        RtuCantGetAppFolder = 1005,
        RtuBaseRefAssignmentError = 1006,
        RtuMonitoringSettingsApplyError = 1007,

        MeasurementError = 1100,
        MeasurementEndedNormally = 1101,
        MeasurementPreparationError = 1102,
        MeasurementInterrupted = 1103,

        TcpConnectionError = 2000,
        C2DWcfConnectionError = 2001,
        C2DWcfOperationError = 2002,
        D2RWcfConnectionError = 2011,
        D2RWcfOperationError = 2012,


        DbError = 3000,
        DbInitializedSuccessfully = 3001,
        DbIsNotInitializedError = 3002,

        BaseRefAssignedSuccessfully = 4001,
        MonitoringSettingsAppliedSuccessfully = 4002,

        NoSuchUserOrWrongPassword = 9001,
        ThisUserRegisteredOnAnotherPc = 9002,
        NoSuchClientStation = 9003,
        ClientRegisteredSuccessfully = 9011,
    }

    public static class ErrorCodeExt
    {
        public static string GetLocalizedString(this ReturnCode returnCode, string exceptionMessage = "")
        {
            switch (returnCode)
            {
                case ReturnCode.Error:
                    return Resources.SID_Error_;

                // 1000
                case ReturnCode.RtuInitializationError:
                    return Resources.SID_RTU_initialization_error_ + $"\n\n{exceptionMessage}";
                case ReturnCode.OtdrInitializationCannotLoadDll:
                    return Resources.SID_Cannot_find_dll_file_;
                case ReturnCode.OtdrInitializationCannotInitializeDll:
                    return Resources.SID_Cannot_initialize_dll_;

                    // 2000
                case ReturnCode.C2DWcfConnectionError:
                    return Resources.SID_Cannot_establish_connection_with_DataCenter_;
                case ReturnCode.C2DWcfOperationError:
                    return "Error during Client-Datacenter connection" + $"\n\n{exceptionMessage}";
                case ReturnCode.D2RWcfConnectionError:
                    return "Cannot establish connection with RTU";
                case ReturnCode.D2RWcfOperationError:
                    return "Error during Datacenter-Rtu connection" + $"\n\n{exceptionMessage}";

                // 3000
                case ReturnCode.DbError:
                    return "Database error!" + $"\n\n{exceptionMessage}";

                // 4000
                case ReturnCode.BaseRefAssignedSuccessfully:
                    return "Base ref(s) are saved successfully.";
                case ReturnCode.MonitoringSettingsAppliedSuccessfully:
                    return "Monitoring settings are applied successfully.";
                    // 9000
                case ReturnCode.NoSuchUserOrWrongPassword:
                    return "No such user or wrong password!";
                case ReturnCode.ThisUserRegisteredOnAnotherPc:
                    return "User with the same name is registered on another PC";
                default: return "Unknown return code";
            }
        }
    }
}
