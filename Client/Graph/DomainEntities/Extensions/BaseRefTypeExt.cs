using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Graph
{
    public static class BaseRefTypeExt
    {
        public static string GetLocalizedString(this BaseRefType baseRefType)
        {
            switch (baseRefType)
            {
                case BaseRefType.Precise: return Resources.SID_Precise;
                case BaseRefType.Fast: return Resources.SID_Fast;
                case BaseRefType.Additional: return Resources.SID_Second;
                default: return "";
            }
        }

        public static string GetLocalizedFemaleString(this BaseRefType baseRefType)
        {
            switch (baseRefType)
            {
                case BaseRefType.Precise: return Resources.SID_PreciseF;
                case BaseRefType.Fast: return Resources.SID_FastF;
                case BaseRefType.Additional: return Resources.SID_SecondF;
                default: return "";
            }
        }

        public static string GetLocalizedGenitiveString(this BaseRefType baseRefType)
        {
            switch (baseRefType)
            {
                case BaseRefType.Precise: return Resources.SID_preciseG;
                case BaseRefType.Fast: return Resources.SID_fastG;
                case BaseRefType.Additional: return Resources.SID_secondG;
                default: return "";
            }
        }

       

        // public static string ToMeasFileName(this BaseRefType baseRefType)
        // {
        //     return ToFileName(baseRefType, SorType.Meas);
        // }

        //public static MeasurementResult ToMeasurementResultProblem(this BaseRefType baseRefType)
        //{
        //    switch (baseRefType)
        //    {
              
        //        case BaseRefType.Fast:
        //            return MeasurementResult.FastBaseRefNotFound;
        //        case BaseRefType.Additional:
        //            return MeasurementResult.AdditionalBaseRefNotFound;
        //        // case BaseRefType.Precise:
        //        default:
        //            return MeasurementResult.PreciseBaseRefNotFound;   }
        //}
    }
}