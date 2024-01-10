namespace Iit.Fibertest.Dto
{
    public static class ReturnCodeExtensions
    {
        public static bool IsRtuStatusEvent(this ReturnCode code)
        {
            return code == ReturnCode.MeasurementBaseRefNotFound
                   || code == ReturnCode.MeasurementFailedToSetParametersFromBase
                   || code == ReturnCode.MeasurementAnalysisFailed
                   || code == ReturnCode.MeasurementComparisonFailed;
        }
    }
}