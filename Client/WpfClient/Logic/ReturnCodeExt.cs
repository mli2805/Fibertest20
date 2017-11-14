using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class ReturnCodeExt
    {
        public static string GetLocalizedString(this ReturnCode returnCode, string exceptionMessage = "")
        {
            switch (returnCode)
            {
                case ReturnCode.Error:
                    return Resources.SID_Error_;

                // 1000
                case ReturnCode.RtuInitializationError:
                    return Resources.SID_RTU_initialization_error_ + $@"\n\n{exceptionMessage}";
                case ReturnCode.OtdrInitializationCannotLoadDll:
                    return Resources.SID_Cannot_find_dll_file_;
                case ReturnCode.OtdrInitializationCannotInitializeDll:
                    return Resources.SID_Cannot_initialize_dll_;

                // 2000
                case ReturnCode.C2DWcfConnectionError:
                    return Resources.SID_Cannot_establish_connection_with_DataCenter_;
                case ReturnCode.C2DWcfOperationError:
                    return Resources.SID_Error_during_Client_Datacenter_connection + $@"\n\n{exceptionMessage}";
                case ReturnCode.D2RWcfConnectionError:
                    return Resources.SID_Cannot_establish_connection_with_RTU;
                case ReturnCode.D2RWcfOperationError:
                    return Resources.SID_Error_during_Datacenter_Rtu_connection + $@"\n\n{exceptionMessage}";

                // 3000
                case ReturnCode.DbError:
                    return Resources.SID_Database_error_ + $@"\n\n{exceptionMessage}";

                // 4000
                case ReturnCode.BaseRefAssignedSuccessfully:
                    return Resources.SID_Base_ref_s__are_saved_successfully_;
                case ReturnCode.MonitoringSettingsAppliedSuccessfully:
                    return Resources.SID_Monitoring_settings_are_applied_successfully_;
                // 9000
                case ReturnCode.NoSuchUserOrWrongPassword:
                    return Resources.SID_No_such_user_or_wrong_password_;
                case ReturnCode.ThisUserRegisteredOnAnotherPc:
                    return Resources.SID_User_with_the_same_name_is_registered_on_another_PC;
                default: return Resources.SID_Unknown_return_code;
            }
        }
    }
}