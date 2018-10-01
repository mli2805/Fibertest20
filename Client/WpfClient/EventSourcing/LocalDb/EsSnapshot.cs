namespace Iit.Fibertest.Client
{
    public class EsSnapshot
    {
        public int Id { get; set; }
        public int LastIncludedEvent { get; set; }
        public byte[] Snapshot { get; set; }
    }
}