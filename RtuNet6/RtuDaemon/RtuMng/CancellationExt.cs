namespace Iit.Fibertest.RtuDaemon
{
    public static class CancellationExt
    {
        public static bool IsCancellationRequested(this CancellationToken[] tokens)
        {
            return tokens.Any(t => t.IsCancellationRequested);
        }
    }
}
