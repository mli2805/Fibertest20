﻿using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.UtilsLib
{
    public enum BwReturnProgressCode
    {
        FilesAreCopiedSuccessfully = 10,
        CopyFilesError,
        ErrorSourceFolderNotFound,
        FilesAreCopied,

        ServiceIsBeingInstalled,
        CannotInstallService,
        ServiceInstalledSuccessfully,
        ServiceIsBeingUninstalled,
        CannotUninstallService,
        ServiceUninstalledSuccessfully,
        ServiceIsBeingStopped,
        CannotStopService,
        ServiceStoppedSuccessfully,

        ShortcutsAreCreatedSuccessfully,

        ClientSetupStarted,
        ClientSetupCompletedSuccessfully,
        DataCenterSetupStarted,
        DataCenterSetupCompletedSuccessfully,
        RtuManagerSetupStarted,
        RtuManagerSetupCompletedSuccessfully,
        UninstallSetupStarted,
        UninstallSetupCompletedSuccessfully,

        UninstallStarted,
        DeletingFiles,
        CannotDeleteSpecifiedFolder,
        FilesAreDeletedSuccessfully,
        ShortcutsDeleted,
        RegistryCleaned,
        UninstallFinished,
    }

    public static class BwReturnProgressCodeExt
    {
        public static string GetLocalizedString(this BwReturnProgressCode code, string addition)
        {
            switch (code)
            {
                case BwReturnProgressCode.FilesAreCopiedSuccessfully:
                    return Resources.SID_Files_are_copied_successfully_;
                case BwReturnProgressCode.CopyFilesError:
                    return string.Format(Resources.SID_Copy_files_error___0_, addition);
                case BwReturnProgressCode.ErrorSourceFolderNotFound:
                    return string.Format(Resources.SID_Error__Source_folder__0__not_found_, addition);
                case BwReturnProgressCode.FilesAreCopied:
                    return Resources.SID_Files_are_copied___;

                case BwReturnProgressCode.ServiceIsBeingInstalled:
                    return string.Format(Resources.SID__0__service_is_being_installed___, addition);
                case BwReturnProgressCode.CannotInstallService:
                    return string.Format(Resources.SID_Cannot_install_service__0_, addition);
                case BwReturnProgressCode.ServiceInstalledSuccessfully:
                    return string.Format(Resources.SID__0__service_installed_successfully, addition);
                case BwReturnProgressCode.ServiceIsBeingUninstalled:
                    return string.Format(Resources.SID__0__service_is_being_uninstalled___, addition);
                case BwReturnProgressCode.CannotUninstallService:
                    return string.Format(Resources.SID_Cannot_uninstall_service__0_, addition);
                case BwReturnProgressCode.ServiceUninstalledSuccessfully:
                    return string.Format(Resources.SID_Service__0__uninstalled_successfully_, addition);
                case BwReturnProgressCode.ServiceIsBeingStopped:
                    return string.Format(Resources.SID__0__service_is_being_stopped___, addition);
                case BwReturnProgressCode.CannotStopService:
                    return string.Format(Resources.SID_Cannot_stop_service__0_, addition);
                case BwReturnProgressCode.ServiceStoppedSuccessfully:
                    return string.Format(Resources.SID_Service__0__stopped_successfully_, addition);

                case BwReturnProgressCode.ShortcutsAreCreatedSuccessfully:
                    return Resources.SID_Shortcuts_are_created_successfully_;

                case BwReturnProgressCode.ClientSetupStarted:
                    return Resources.SID_Client_setup_started_;
                case BwReturnProgressCode.ClientSetupCompletedSuccessfully:
                    return Resources.SID_Client_setup_completed_successfully_;
                case BwReturnProgressCode.DataCenterSetupStarted:
                    return Resources.SID_Data_Center_setup_started_;
                case BwReturnProgressCode.DataCenterSetupCompletedSuccessfully:
                    return Resources.SID_Data_Center_setup_completed_successfully_;
                case BwReturnProgressCode.RtuManagerSetupStarted:
                    return Resources.SID_RTU_Manager_setup_started_;
                case BwReturnProgressCode.RtuManagerSetupCompletedSuccessfully:
                    return Resources.SID_RTU_Manager_setup_completed_successfully_;
                case BwReturnProgressCode.UninstallSetupStarted:
                    return Resources.SID_Uninstall_setup_started_;
                case BwReturnProgressCode.UninstallSetupCompletedSuccessfully:
                    return Resources.SID_Uninstall_setup_completed_successfully_;

                case BwReturnProgressCode.UninstallStarted:
                    return Resources.SID_Uninstall_started___;
                case BwReturnProgressCode.DeletingFiles:
                    return Resources.SID_Deleting_files___;
                case BwReturnProgressCode.CannotDeleteSpecifiedFolder:
                    return Resources.SID_Cannot_delete_specified_folder_;
                case BwReturnProgressCode.FilesAreDeletedSuccessfully:
                    return Resources.SID_Files_are_deleted_successfully_;
                case BwReturnProgressCode.ShortcutsDeleted:
                    return Resources.SID_Shortcuts_deleted_;
                case BwReturnProgressCode.RegistryCleaned:
                    return Resources.SID_Registry_cleaned_;
                case BwReturnProgressCode.UninstallFinished:
                    return Resources.SID_Uninstall_finished_;
            }

            return "";
        }
    }
}