using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public enum MeasurementCompletedStatus
    {
        MeasurementClientStartedSuccessfully,
        MeasurementCompletedSuccessfully,
        BaseRefAssignedSuccessfully,

        RtuInitializationInProgress,
        RtuAutoBaseMeasurementInProgress,
        InvalidValueOfLmax,
        SnrIs0,
        MeasurementError,
        MeasurementTimeoutExpired,
        FetchMeasurementFromRtu4000Failed,
        BaseRefAssignmentFailed,

        Unknown,
    }

    public static class MeasurementCompletedStatusExt
    {
        public static MeasurementCompletedStatus Convert(this ReturnCode returnCode)
        {
            switch (returnCode)
            {
                case ReturnCode.MeasurementClientStartedSuccessfully:
                    return MeasurementCompletedStatus.MeasurementClientStartedSuccessfully;
                case ReturnCode.MeasurementEndedNormally:
                    return MeasurementCompletedStatus.MeasurementCompletedSuccessfully;
                case ReturnCode.BaseRefAssignedSuccessfully:
                    return MeasurementCompletedStatus.BaseRefAssignedSuccessfully;
                case ReturnCode.RtuInitializationInProgress:
                    return MeasurementCompletedStatus.RtuInitializationInProgress;
                case ReturnCode.RtuAutoBaseMeasurementInProgress:
                    return MeasurementCompletedStatus.RtuAutoBaseMeasurementInProgress;
                case ReturnCode.InvalidValueOfLmax:
                    return MeasurementCompletedStatus.InvalidValueOfLmax;
                case ReturnCode.SnrIs0:
                    return MeasurementCompletedStatus.SnrIs0;
                case ReturnCode.MeasurementError:
                    return MeasurementCompletedStatus.MeasurementError;
                case ReturnCode.FetchMeasurementFromRtu4000Failed:
                    return MeasurementCompletedStatus.FetchMeasurementFromRtu4000Failed;
                case ReturnCode.BaseRefAssignmentFailed:
                    return MeasurementCompletedStatus.BaseRefAssignmentFailed;

                default: return MeasurementCompletedStatus.Unknown;
            }
        }

        public static string GetLocalizedString(this MeasurementCompletedStatus status)
        {
            switch (status)
            {
                case MeasurementCompletedStatus.MeasurementClientStartedSuccessfully:
                    return @"Measurement(Client) started."; 
                case MeasurementCompletedStatus.MeasurementCompletedSuccessfully:
                    return Resources.SID_Measurement_completed_successfully;
                case MeasurementCompletedStatus.BaseRefAssignedSuccessfully:
                    return Resources.SID_Base_refs_assigned_successfully;

                case MeasurementCompletedStatus.RtuInitializationInProgress:
                    return @"RTU initialization in progress";
                case MeasurementCompletedStatus.RtuAutoBaseMeasurementInProgress:
                    return @"Auto base measurement in progress";
                case MeasurementCompletedStatus.InvalidValueOfLmax:
                    return Resources.SID_Failed_to_automatically_determine_the_correct_measurement_parameters;
                case MeasurementCompletedStatus.SnrIs0:
                    return Resources.SID_No_fiber;
                case MeasurementCompletedStatus.MeasurementError:
                    return @"Measurement error";
                case MeasurementCompletedStatus.MeasurementTimeoutExpired:
                    return Resources.SID_Measurement_timeout_expired;
                case MeasurementCompletedStatus.FetchMeasurementFromRtu4000Failed:
                    return Resources.SID_Failed_to_fetch_measurement_from_Rtu4000;

                case MeasurementCompletedStatus.BaseRefAssignmentFailed:
                    return Resources.SID_Failed_to_assign_as_base;
            }

            return @"Unknown status";
        }

        public static string KhazanovStyle(this MeasurementCompletedStatus status)
        {
            switch (status)
            {
                case MeasurementCompletedStatus.MeasurementClientStartedSuccessfully:
                    return @"Measurement started";
                case MeasurementCompletedStatus.MeasurementCompletedSuccessfully:
                    return Resources.SID_Measurement___successfully;
                case MeasurementCompletedStatus.BaseRefAssignedSuccessfully:
                    return Resources.SID_Assignment___successfully;

                case MeasurementCompletedStatus.RtuInitializationInProgress:
                case MeasurementCompletedStatus.RtuAutoBaseMeasurementInProgress:
                case MeasurementCompletedStatus.InvalidValueOfLmax:
                case MeasurementCompletedStatus.SnrIs0:
                case MeasurementCompletedStatus.MeasurementError:
                case MeasurementCompletedStatus.MeasurementTimeoutExpired:
                case MeasurementCompletedStatus.FetchMeasurementFromRtu4000Failed:
                    return Resources.SID_Measurement___failed;

                case MeasurementCompletedStatus.BaseRefAssignmentFailed:
                    return Resources.SID_Assignment___failed;
            }
            return @"Unknown status";
        }
    }
}