namespace Iit.Fibertest.InstallLib
{
    public enum BwReturnProgressCode
    {
        FilesAreCopiedSuccessfully = 10,
        CopyFilesError,
        ErrorSourceFolderNotFound,
        FilesAreBeingCopied,
        AntiGhostSettingFailed,

        FilesAreUnziped,
        FilesAreUnzipedSuccessfully,
        ErrorFilesUnzipped,

        ServiceIsBeingInstalled,
        CannotInstallService,
        CannotChangeServiceFailureAction,
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
        WebComponentsSetupStarted,
        WebComponentsSetupCompletedSuccessfully,
        RtuManagerSetupStarted,
        RtuManagerSetupCompletedSuccessfully,
        SuperClientSetupStarted,
        SuperClientSetupCompletedSuccessfully,
        UninstallSetupStarted,
        UninstallSetupCompletedSuccessfully,

        IisOperationError,
        SiteInstalledSuccessfully,
        SiteInstallationError,
        SiteUninstalledSuccessfully,
        SiteUninstallationError,

        UninstallStarted,
        DeletingFiles,
        CannotDeleteSpecifiedFolder,
        FilesAreDeletedSuccessfully,
        ShortcutsDeleted,
        RegistryCleaned,
        UninstallFinished,
    }
}
