using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Graph.RtuOccupy
{
    public static class RtuOccupationStateExt
    {
        public static string GetLocalized(this RtuOccupationState state)
        {
            switch (state.RtuOccupation)
            {
                case RtuOccupation.Initialization:
                    return Resources.SID_RTU_initialization_in_progress;
                case RtuOccupation.AutoBaseMeasurement:
                    return Resources.SID_Auto_base_measurement_in_progress;
                default:
                    return @"Unknown occupation state";
            }
        }
    }
}
