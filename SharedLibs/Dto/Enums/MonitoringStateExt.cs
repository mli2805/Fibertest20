using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Dto
{
    public static class MonitoringStateExt
    {
        public static string ToLocalizedString(this MonitoringState state)
        {
            switch (state)
            {
                case MonitoringState.Off: return Resources.SID_Manual;
                case MonitoringState.On: return Resources.SID_Automatic;
                default: return null;
            }
        }
    }
}