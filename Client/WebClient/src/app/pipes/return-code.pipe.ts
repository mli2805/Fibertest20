import { Pipe, PipeTransform } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { ReturnCode } from "../models/enums/returnCode";

@Pipe({
  name: "ReturnCodeToLocalizedStringPipe",
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
        return this.ts.instant("SID_RTU_initialization_error_");
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

      case ReturnCode.RtuIsBusy:
        return this.ts.instant("SID_RTU_is_busy");
      case ReturnCode.RtuInitializationInProgress:
        return this.ts.instant("SID_RTU_initialization_in_progress");
      case ReturnCode.RtuAutoBaseMeasurementInProgress:
        return this.ts.instant("SID_Auto_base_measurement_in_progress");

      case ReturnCode.RtuAttachOtauError:
        return this.ts.instant("SID_Attach_OTAU_error_");
      case ReturnCode.RtuDetachOtauError:
        return this.ts.instant("SID_Failed_to_detach_additional_otau_");
      case ReturnCode.RtuMonitoringSettingsApplyError:
        return this.ts.instant("SID_Failed_to_apply_monitoring_settings_");

      case ReturnCode.RtuToggleToPortError:
      case ReturnCode.RtuToggleToBopPortError:
          return this.ts.instant("SID_Failed_to_toggle_to_port");
      case ReturnCode.InvalidValueOfLmax:
          return this.ts.instant("SID_Failed_to_automatically_determine_the_correct_measurement_parameters");
      case ReturnCode.SnrIs0:
          return this.ts.instant("SID_No_fiber");

       // 1500
      case ReturnCode.MeasurementError:
          return this.ts.instant("SID_Measurement_error");
      case ReturnCode.MeasurementEndedNormally:
          return this.ts.instant("SID_Measurement_completed_successfully");
      case ReturnCode.MeasurementPreparationError:
          return this.ts.instant("SID_Measurement_preparation_error");
      case ReturnCode.MeasurementBaseRefNotFound:
          return this.ts.instant("SID_base_ref_not_found");
      case ReturnCode.MeasurementFailedToSetParametersFromBase:
          return this.ts.instant("SID_Failed_to_set_parameters_from__0__base");
      case ReturnCode.MeasurementInterrupted:
          return this.ts.instant("SID_Measurement_interrupted");
      case ReturnCode.MeasurementTimeoutExpired:
          return this.ts.instant("SID_Measurement_timeout_expired");
      case ReturnCode.RtuFrequentServiceRestarts:
          return this.ts.instant("SID_Frequent_service_restarts");
      case ReturnCode.RtuManagerServiceWorking:
          return this.ts.instant("SID_Service_is_working___RTU_Manager_");

      // 2000
      case ReturnCode.C2DWcfConnectionError:
        return this.ts.instant(
          "SID_Cannot_establish_connection_with_DataCenter_"
        );
      case ReturnCode.C2DWcfOperationError:
        return this.ts.instant("SID_Error_during_Client_Datacenter_connection");
      case ReturnCode.D2RWcfConnectionError:
        return this.ts.instant("SID_Cannot_establish_connection_with_RTU_");
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

      // 5000
      case ReturnCode.LandmarkChangesAppliedSuccessfully:
        return this.ts.instant("SID_Landmark_changes_applied_successfully");
      case ReturnCode.FailedToApplyLandmarkChanges:
        return this.ts.instant("SID_Failed_to_apply_landmark_changes");
      case ReturnCode.FailedToGetBaseRefs:
        return this.ts.instant("SID_Failed_to_get_base_refs");
      case ReturnCode.BaseRefsForTraceModifiedSuccessfully:
        return this.ts.instant("SID_Base_refs_for_trace_modified_successfully");
      case ReturnCode.FailedToModifyBaseRef:
        return this.ts.instant("SID_Failed_to_modify_base_ref");
      case ReturnCode.BaseRefsSavedSuccessfully:
        return this.ts.instant("SID_Base_refs_saved_successfully");
      case ReturnCode.FailedToSaveBaseRefs:
        return this.ts.instant("SID_Failed_to_save_base_refs");
      case ReturnCode.BaseRefsForTraceSentSuccessfully:
        return this.ts.instant("SID_Base_refs_for_trace_sent_successfully");
      case ReturnCode.FailedToSendBaseToRtu:
        return this.ts.instant("SID_Failed_to_send_base_To_RTU");
      case ReturnCode.FailedToUpdateVeexTestList:
        return this.ts.instant("SID_Failed_to_update_VEEX_test_list");
      case ReturnCode.BaseRefsAmendmentProcessDone:
        return this.ts.instant("SID_Base_refs_amendment_process_is_finished");

      
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
      case ReturnCode.VersionsDoNotMatch:
        return this.ts.instant("SID_Versions_do_not_match");
      // 9401
      case ReturnCode.BaseRefAssignmentFailed:
        return this.ts.instant("SID_Base_reference_assignment_failed");
      case ReturnCode.BaseRefAssignmentParamNotAcceptable:
        return this.ts.instant(
          "SID_Measurement_parameters_are_not_compatible_with_this_RTU"
        );
      case ReturnCode.BaseRefAssignmentNoThresholds:
        return this.ts.instant("SID_There_are_no_thresholds_for_comparison");
      case ReturnCode.BaseRefAssignmentLandmarkCountWrong:
        return this.ts.instant("SID_Landmark_count_does_not_match_graph");
      case ReturnCode.BaseRefAssignmentEdgeLandmarksWrong:
        return this.ts.instant(
          "SID_First_and_last_landmarks_should_be_associated_with_key_events_"
        );
      case ReturnCode.RtuBaseRefAssignmentError:
        return "RTU failed to save base ref";

      default:
        return this.ts.instant("SID_Unknown_return_code") + ":  " + value;
    }
  }
}
