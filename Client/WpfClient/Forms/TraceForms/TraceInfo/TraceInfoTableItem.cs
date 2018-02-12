namespace Iit.Fibertest.Client
{
    public class TraceInfoTableItem
    {
        public string NodeType { get; set; }
        public int Count { get; set; }

        public TraceInfoTableItem(string nodeType, int count)
        {
            NodeType = nodeType;
            Count = count;
        }
    }
}