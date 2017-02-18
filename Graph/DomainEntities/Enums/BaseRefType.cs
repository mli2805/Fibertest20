namespace Iit.Fibertest.Graph
{
    public enum BaseRefType
    {
        None,
        Precise,
        Fast,
        Additional
    }

    public enum RtuPartState
    {
        Broken = -1,
        None   =  0, 
        Normal,
    }

    public enum MonitoringState
    {
        Off,
        OffButReady,  // only for trace
        On,
    }
}