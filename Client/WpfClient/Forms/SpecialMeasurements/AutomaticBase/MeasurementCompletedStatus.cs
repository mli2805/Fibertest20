namespace Iit.Fibertest.Client
{
    public enum MeasurementCompletedStatus
    {
        MeasurementCompletedSuccessfully,
        BaseRefAssignedSuccessfully,

        FailedToStart,
        MeasurementTimeoutExpired,
        FailedToFetchFromRtu4000,
        FailedToAssignAsBase,
    }
}