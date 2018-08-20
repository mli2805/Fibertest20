namespace Iit.Fibertest.Dto
{
    public enum ReturnCode
    {
        Error = 0,
        Ok = 1,

        RtuInitializationError = 1000,
        RtuInitializedSuccessfully = 1001,
        OtdrInitializationCannotLoadDll = 1002,
        OtdrInitializationCannotInitializeDll = 1003,
        OtdrCannontConnect = 1004,
        OtauInitializationError = 1005,

        RtuDoesntSupportBop = 1012,
        RtuTooBigPortNumber = 1013,

        RtuIsBusy = 1100,
        RtuBaseRefAssignmentError = 1106,
        RtuMonitoringSettingsApplyError = 1107,
        RtuAttachOtauError = 1108,
        RtuDetachOtauError = 1109,
        RtuToggleToPortError = 1110,

        MeasurementError = 1500,
        MeasurementEndedNormally = 1501,
        MeasurementPreparationError = 1502,
        MeasurementInterrupted = 1503,

        TcpConnectionError = 2000,
        C2DWcfConnectionError = 2001,
        C2DWcfOperationError = 2002,
        D2RWcfConnectionError = 2011,
        D2RWcfOperationError = 2012,

        DbError = 3000,
        DbInitializedSuccessfully = 3001,
        DbIsNotInitializedError = 3002,
        DbCannotConvertThisReSendToAssign = 3003,
        DbEntityToUpdateNotFound = 3004,

        BaseRefAssignedSuccessfully = 4001,
        MonitoringSettingsAppliedSuccessfully = 4002,
        OtauAttachedSuccesfully = 4003,
        OtauDetachedSuccesfully = 4004,

        NoSuchUserOrWrongPassword = 9001,
        ThisUserRegisteredOnAnotherPc = 9002,
        NoSuchClientStation = 9003,
        NoSuchRtu = 9004,
        ClientsCountExceeded = 9005,
        ClientsCountLicenseExpired = 9006,
        SuperClientsCountExceeded = 9007,
        SuperClientsCountLicenseExpired = 9008,
        ClientRegisteredSuccessfully = 9011,
    }
}
