namespace Iit.Fibertest.Client
{
    public class NodesStatisticsItem
    {
        public string NodeType { get; set; }
        public int Count { get; set; }

        public NodesStatisticsItem(string nodeType, int count)
        {
            NodeType = nodeType;
            Count = count;
        }
    }
}