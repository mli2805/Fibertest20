import { Pipe, PipeTransform } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { ReturnCode } from "../models/enums/returnCode";

@Pipe({
  name: "ReturnCodeToLocalizedStringPipe"
})
export class ReturnCodePipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: ReturnCode): string {
    switch (value) {
      case ReturnCode.Ok:
        return "OK";
      case ReturnCode.Error:
        return this.ts.instant("SID_Error_");

      // 1000
      case ReturnCode.RtuInitializationError:
        return this.ts.instant("SID_RTU_initialization_error_ ");
      case ReturnCode.RtuInitializedSuccessfully:
        return this.ts.instant("SID_RTU_initialized_successfully_");
      case ReturnCode.OtdrInitializationCannotLoadDll:
        return this.ts.instant("SID_Cannot_find_dll_file_");
      case ReturnCode.OtdrInitializationCannotInitializeDll:
        return this.ts.instant("SID_Cannot_initialize_dll_");
      case ReturnCode.RtuDoesntSupportBop:
        return this.ts.instant("SID_RTU_does_not_support_BOP");
      case ReturnCode.RtuTooBigPortNumber:
        return this.ts.instant("SID_Too_big_port_number_for_BOP_attachment");
      case ReturnCode.RtuAttachOtauError:
        return this.ts.instant("SID_Attach_OTAU_error_");

      // 2000
      case ReturnCode.C2DWcfConnectionError:
        return this.ts.instant(
          "SID_Cannot_establish_connection_with_DataCenter_"
        );
      case ReturnCode.C2DWcfOperationError:
        return this.ts.instant("SID_Error_during_Client_Datacenter_connection");
      case ReturnCode.D2RWcfConnectionError:
        return this.ts.instant("SID_Cannot_establish_connection_with_RTU");
      case ReturnCode.D2RWcfOperationError:
        return this.ts.instant("SID_Error_during_Datacenter_Rtu_connection");

      // 3000
      case ReturnCode.DbError:
        return this.ts.instant("SID_Database_error_");

      // 4000
      case ReturnCode.BaseRefAssignedSuccessfully:
        return this.ts.instant("SID_Base_ref_s__are_saved_successfully_");
      case ReturnCode.MonitoringSettingsAppliedSuccessfully:
        return this.ts.instant(
          "SID_Monitoring_settings_are_applied_successfully_"
        );
      // 9000
      case ReturnCode.ClientRegisteredSuccessfully:
        return "OK";
      case ReturnCode.NoSuchUserOrWrongPassword:
        return this.ts.instant("SID_No_such_user_or_wrong_password_");
      case ReturnCode.ThisUserRegisteredFromAnotherDevice:
        return this.ts.instant(
          "SID_User_with_the_same_name_is_registered_on_another_PC"
        );
      case ReturnCode.NoSuchClientStation:
        return "No such client station";
      case ReturnCode.ClientsCountExceeded:
        return this.ts.instant(
          "SID_Exceeded_the_number_of_clients_registered_simultaneously"
        );
      case ReturnCode.ClientsCountLicenseExpired:
        return this.ts.instant("SID_Clients_license_is_expired");
      case ReturnCode.WebClientsCountExceeded:
        return this.ts.instant(
          "SID_Exceeded_the_number_of_web_clients_registered_simultaneously"
        );
      case ReturnCode.WebClientsCountLicenseExpired:
        return this.ts.instant("SID_Web_clients_license_is_expired");
      case ReturnCode.SuperClientsCountExceeded:
        return this.ts.instant(
          "SID_Exceeded_the_number_of_super_clients_registered_simultaneously"
        );
      case ReturnCode.SuperClientsCountLicenseExpired:
        return this.ts.instant("SID_Super_clients_license_is_expired");
      case ReturnCode.UserHasNoRightsToStartClient:
        return this.ts.instant("SID_This_user_has_no_right_to_start_client");
      case ReturnCode.SuperClientsCountExceeded:
        return this.ts.instant(
          "SID_Exceeded_the_number_of_super_clients_registered_simultaneously"
        );
      case ReturnCode.UserHasNoRightsToStartClient:
        return this.ts.instant("SID_This_user_has_no_right_to_start_client");
      case ReturnCode.UserHasNoRightsToStartSuperClient:
        return this.ts.instant(
          "SID_This_user_has_no_right_to_start_SuperClient"
        );
      case ReturnCode.UserHasNoRightsToStartWebClient:
        return this.ts.instant(
          "SID_This_user_has_no_right_to_start_Web_Client"
        );
      // 9401
      case ReturnCode.BaseRefAssignmentFailed:
        return this.ts.instant("SID_Base_reference_assignment_failed");

      default:
        return this.ts.instant("SID_Unknown_return_code") + ":  " + value;
    }
  }
}
