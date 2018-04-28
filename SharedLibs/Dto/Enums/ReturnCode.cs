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
        RtuIsBusy = 1004,
        RtuCantGetAppFolder = 1005,
        RtuBaseRefAssignmentError = 1006,
        RtuMonitoringSettingsApplyError = 1007,
        RtuAttachOtauError = 1008,
        RtuDetachOtauError = 1009,
        RtuToggleToPortError = 1010,
        RtuMeasurementError = 1011,
        RtuDoesntSupportBop = 1012,
        RtuTooBigPortNumber = 1013,

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
        ClientRegisteredSuccessfully = 9011,
    }
}
