namespace Iit.Fibertest.Graph
{
    public enum MonitoringState
    {
        Off,
        On,
    }

    public static class MonitoringStateExt
    {
        public static string ToLocalizedString(this MonitoringState state)
        {
            switch (state)
            {
                    case MonitoringState.Off: return "Manual";
                    case MonitoringState.On: return "Automatic";
                    default: return null;
            }
        }
    }
}