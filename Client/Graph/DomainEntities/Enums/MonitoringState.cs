namespace Iit.Fibertest.Graph
{
    public enum MonitoringState
    {
        Off,
        OffButReady,  // only for traces which were included in monitoring cycle but now RTU's monitoring state is Off
        On,
    }
}