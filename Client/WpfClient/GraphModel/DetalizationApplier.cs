using System.Threading.Tasks;

namespace Iit.Fibertest.Client
{
    public static class DetalizationApplier
    {
        private const int Portion = 50;
        private const int Delay = 2;

        public static async Task ToEmptyGraph(this GraphReadModel graphReadModel, RenderingResult renderingResult)
        {
            var i = 0;
            while (i < renderingResult.NodeVms.Count)
            {
                for (int j = 0; j < Portion; j++)
                {
                    if (i >= renderingResult.NodeVms.Count) break;
                    var nodeVm = renderingResult.NodeVms[i];
                    graphReadModel.Data.Nodes.Add(nodeVm);
                    i++;
                }
                await Task.Delay(Delay);
            }

            i = 0;
            while (i < renderingResult.FiberVms.Count)
            {
                for (int j = 0; j < Portion; j++)
                {
                    if (i >= renderingResult.FiberVms.Count) break;
                    var fiberVm = renderingResult.FiberVms[i];
                    graphReadModel.Data.Fibers.Add(fiberVm);
                    i++;
                }
                await Task.Delay(Delay);
            }
        }

        public static async Task ToExistingGraph(this GraphReadModel graphReadModel, RenderingResult renderingResult)
        {

        }
    }
}
