using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GraphGpsCalculator
    {
        private readonly ReadModel _readModel;

        public GraphGpsCalculator(ReadModel readModel)
        {
            _readModel = readModel;
        }

        public double CalculateTraceGpsLength(Trace trace)
        {
            double result = 0;
            for (int i = 0; i < trace.Nodes.Count-1; i++)
            {
                var node1 = _readModel.Nodes.FirstOrDefault(n => n.Id == trace.Nodes[i]);
                if (node1 == null) return 0;
                var node2 = _readModel.Nodes.FirstOrDefault(n => n.Id == trace.Nodes[i+1]);
                if (node2 == null) return 0;

                result = result + GpsCalculator.CalculateGpsDistance(node1.Latitude, node1.Longitude, node2.Latitude, node2.Longitude);
            }
            return result;
        }
    }
}