namespace Iit.Fibertest.RtuMngr
{
    public static class CancellationExt
    {
        public static bool IsCancellationRequested(this CancellationToken[] tokens)
        {
            return tokens.Any(t => t.IsCancellationRequested);
        }
    }
}
