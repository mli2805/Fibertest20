using System.Threading.Tasks;

namespace Iit.Fibertest.Client
{
    public static class GraphReadModelRenderingExt
    {
        public static async Task<int> FullClean(this GraphReadModel graphReadModel)
        {
            await Task.Delay(2);
            graphReadModel.Data.Fibers.Clear();
            await Task.Delay(2);
            graphReadModel.Data.Nodes.Clear();
            await Task.Delay(2);
            if (graphReadModel.MainMap == null) return 0; // under tests
            for (int i = graphReadModel.MainMap.Markers.Count - 1; i >= 0; i--)
            {
                graphReadModel.MainMap.Markers.RemoveAt(i);
                if (i % 100 == 0) await Task.Delay(2);
            }

            return 1;
        }

    }
}
