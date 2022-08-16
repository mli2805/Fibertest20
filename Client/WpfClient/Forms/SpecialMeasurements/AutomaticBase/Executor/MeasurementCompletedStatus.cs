using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public enum MeasurementCompletedStatus
    {
        MeasurementCompletedSuccessfully,
        BaseRefAssignedSuccessfully,

        FailedToStart,
        InvalidValueLmax,
        MeasurementTimeoutExpired,
        FailedToFetchFromRtu4000,
        FailedToAssignAsBase,
    }

    public static class MeasurementCompletedStatusExt
    {
        public static string GetLocalizedString(this MeasurementCompletedStatus status)
        {
            switch (status)
            {
                case MeasurementCompletedStatus.MeasurementCompletedSuccessfully:
                    return Resources.SID_Measurement_completed_successfully;
                case MeasurementCompletedStatus.BaseRefAssignedSuccessfully:
                    return Resources.SID_Base_refs_assigned_successfully;

                case MeasurementCompletedStatus.FailedToStart:
                    return Resources.SID_Failed_to_start;
                case MeasurementCompletedStatus.InvalidValueLmax:
                    return Resources.SID_Failed_to_automatically_determine_the_correct_measurement_parameters;
                case MeasurementCompletedStatus.MeasurementTimeoutExpired:
                    return Resources.SID_Measurement_timeout_expired;
                case MeasurementCompletedStatus.FailedToFetchFromRtu4000:
                    return Resources.SID_Failed_to_fetch_measurement_from_Rtu4000;

                case MeasurementCompletedStatus.FailedToAssignAsBase:
                    return Resources.SID_Failed_to_assign_as_base;
            }

            return @"Unknown status";
        }

        public static string KhazanovStyle(this MeasurementCompletedStatus status)
        {
            switch (status)
            {
                case MeasurementCompletedStatus.MeasurementCompletedSuccessfully:
                    return Resources.SID_Measurement___successfully;
                case MeasurementCompletedStatus.BaseRefAssignedSuccessfully:
                    return Resources.SID_Assignment___successfully;

                case MeasurementCompletedStatus.FailedToStart:
                case MeasurementCompletedStatus.InvalidValueLmax:
                case MeasurementCompletedStatus.MeasurementTimeoutExpired:
                case MeasurementCompletedStatus.FailedToFetchFromRtu4000:
                    return Resources.SID_Measurement___failed;

                case MeasurementCompletedStatus.FailedToAssignAsBase:
                    return Resources.SID_Assignment___failed;
            }
            return @"Unknown status";
        }
    }
}