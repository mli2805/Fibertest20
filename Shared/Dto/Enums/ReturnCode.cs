namespace Iit.Fibertest.Dto
{
    public enum ReturnCode
    {
        Error = 0x0,
        Ok = 1,
        InProgress = 2,
        TimeOutExpired = 3,
        NotFound = 4,
        WrongDataCenter = 5, // not the same server that has initialized this RTU earlier

        ClientCleanedAsDead = 10,

        RtuInitializationError = 1000,
        RtuInitializedSuccessfully = 1001,
        OtdrInitializationCannotLoadDll = 1002,
        OtdrInitializationCannotInitializeDll = 1003,
        FailedToConnectOtdr = 1004,
        OtauInitializationError = 1005,
        CharonComPortError = 1006,
        OtdrInitializationFailed = 1007, // Veex
        RtuUnauthorizedAccess = 1008,

        RtuDoesNotSupportBop = 1012,
        RtuTooBigPortNumber = 1013,

        RtuIsBusy = 1100,
        RtuInitializationInProgress = 1101,
        RtuAutoBaseMeasurementInProgress = 1102,
        RtuBaseRefAssignmentError = 1106,
        RtuMonitoringSettingsApplyError = 1107,
        RtuAttachOtauError = 1108,
        RtuDetachOtauError = 1109,
        RtuToggleToPortError = 1110,
        RtuToggleToBopPortError = 1111,
        InvalidValueOfLmax = 1121,
        SnrIs0 = 1122,

        MeasurementError = 1500,
        MeasurementEndedNormally = 1501,
        MeasurementErrorCleared = 1502, // by Initialization + RTU serial changed
        MeasurementErrorClearedByInit = 1503, // RTU the same
        MeasurementPreparationError = 1510,
        MeasurementToggleToPortFailed = 1511,
        MeasurementBaseRefNotFound = 1512,
        MeasurementFailedToSetParametersFromBase = 1513,
        MeasurementHardwareProblem = 1520,
        MeasurementAnalysisFailed = 1530,
        MeasurementComparisonFailed = 1532,
        MeasurementInterrupted = 1590,
        MeasurementTimeoutExpired = 1595,

        RtuFrequentServiceRestarts = 1600,
        RtuManagerServiceWorking = 1601,

        TcpConnectionError = 2000,
        C2DWcfConnectionError = 2001,
        C2DWcfOperationError = 2002,
        D2RWcfConnectionError = 2011,
        D2RWcfOperationError = 2012,
        C2RWcfConnectionError = 2021,
        C2RWcfOperationError  = 2022,

        D2RHttpError = 2100,

        SerializationError = 2201,
        DeserializationError = 2202,
        UnknownCommand = 2211,

        DbError = 3000,
        DbInitializedSuccessfully = 3001,
        DbIsNotInitializedError = 3002,
        DbCannotConvertThisReSendToAssign = 3003,
        DbEntityToUpdateNotFound = 3004,

        BaseRefAssignedSuccessfully = 4001,
        OtauAttachedSuccessfully = 4003,
        OtauDetachedSuccessfully = 4004,

        LandmarkChangesAppliedSuccessfully = 5000,
        FailedToApplyLandmarkChanges = 5001,
        FailedToGetBaseRefs = 5011,
        BaseRefsForTraceModifiedSuccessfully = 5020,
        FailedToModifyBaseRef = 5021,
        BaseRefsSavedSuccessfully = 5030,
        FailedToSaveBaseRefs = 5031,
        BaseRefsForTraceSentSuccessfully = 5040,
        FailedToSendBaseToRtu = 5041,
        FailedToUpdateVeexTestList = 5042,
        BaseRefsAmendmentProcessDone = 5050,
        RtuInitializedAndBaseRefsResentSuccessfully = 5100,
        RtuInitializedFailedToReSendBaseRefs = 5101,
        MonitoringSettingsAppliedSuccessfully = 5110,
        FailedToApplyMonitoringSettings = 5111,


        ClientRegisteredSuccessfully = 9000,
        NoSuchUserOrWrongPassword = 9001,
        ThisUserRegisteredFromAnotherDevice = 9002,
        NoSuchClientStation = 9003,
        NoSuchRtu = 9004,
        ClientsCountExceeded = 9005,
        ClientsCountLicenseExpired = 9006,
        WebClientsCountExceeded,
        WebClientsCountLicenseExpired,
        SuperClientsCountExceeded,
        SuperClientsCountLicenseExpired,
        UserHasNoRightsToStartClient,
        UserHasNoRightsToStartSuperClient,
        UserHasNoRightsToStartWebClient,
        VersionsDoNotMatch,
        WrongMachineKey = 9051,
        WrongSecurityAdminPassword = 9052,
        EmptyMachineKey,
        SaveUsersMachineKey,
        NoLicenseHasBeenAppliedYet = 9099,
        NoSuchVeexTest,

        BaseRefAssignmentFailed = 9401,
        BaseRefAssignmentParamNotAcceptable = 9402,
        BaseRefAssignmentNoThresholds = 9403,
        BaseRefAssignmentLandmarkCountWrong = 9404,
        BaseRefAssignmentEdgeLandmarksWrong = 9405,

        FirstLicenseKeyMustNotBeIncremental = 9501,
        LicenseCouldNotBeAppliedRepeatedly,
        LicenseCouldNotBeApplied,

        MeasurementClientStartedSuccessfully = 9601,
        FetchMeasurementFromRtu4000Failed = 9632,
    }
}
