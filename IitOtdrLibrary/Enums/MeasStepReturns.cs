namespace IitOtdrLibrary
{
    public enum MeasStepReturns
    {
        IitReturnXxx = 10000,
        ReturnFinish = 10001,
        ReturnNolink = 10002,
        ReturnNoactlevel = 10003,
        ReturnFiberbreak = 10004,
    }

    public enum ComparisonReturns
    {
        Ok                     = 0,
        NoBase                 = 71,
        DoingAutoMonitor       = 81,
        NoService              = 82,
        NoLink                 = 10002,
        Noactlevel             = 10003,
        FiberBreak             = 10004,
    }
}