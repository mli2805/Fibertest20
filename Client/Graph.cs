using System.Collections.Generic;

namespace Client
{
    public class Graph
    {
        public List<Node> Nodes { get; set; }

    }

    public class Node
    {
        public Coordinates Coordinates { get; set; }
    }
}
