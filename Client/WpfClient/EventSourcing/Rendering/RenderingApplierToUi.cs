using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class RenderingApplierToUi
    {
        private const int Portion = 50;
        private const int Delay = 2;
        private readonly IMyLog _logFile;
        private readonly GraphReadModel _graphReadModel;

        public RenderingApplierToUi(IMyLog logFile, GraphReadModel graphReadModel)
        {
            _logFile = logFile;
            _graphReadModel = graphReadModel;
        }

        public async Task<int> ToEmptyGraph(RenderingResult renderingResult)
        {
            _logFile.AppendLine($@"{renderingResult.NodeVms.Count} nodes;  {renderingResult.FiberVms.Count} fibers");

            var i = 0;
            while (i < renderingResult.NodeVms.Count)
            {
                for (int j = 0; j < Portion; j++)
                {
                    if (i >= renderingResult.NodeVms.Count) break;
                    var nodeVm = renderingResult.NodeVms[i];
                    _graphReadModel.Data.Nodes.Add(nodeVm);
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
                    _graphReadModel.Data.Fibers.Add(fiberVm);
                    i++;
                }
                await Task.Delay(Delay);
            }

            _logFile.AppendLine(@"Rendering applied");
            return 1;
        }

        // public async Task<int> ToExistingGraph(RenderingResult renderingResult)
        // {
        //     await RemoveElementsOfHiddenTraces(renderingResult);
        //
        //     await AddElementsOfShownTraces(renderingResult);
        //
        //     _logFile.AppendLine(@"Rendering applied");
        //     return 1;
        // }

        // private async Task AddElementsOfShownTraces(RenderingResult renderingResult)
        // {
        //     // add nodes for added traces
        //     var i = 0;
        //     while (i < renderingResult.NodeVms.Count)
        //     {
        //         for (int j = 0; j < Portion; j++)
        //         {
        //             if (i >= renderingResult.NodeVms.Count) break;
        //             var nodeVm = renderingResult.NodeVms[i];
        //             var nodeVmInGraph = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == nodeVm.Id);
        //             if (nodeVmInGraph == null)
        //                 _graphReadModel.Data.Nodes.Add(nodeVm);
        //             i++;
        //         }
        //
        //         await Task.Delay(Delay);
        //     }
        //
        //     // add fibers for added traces
        //     i = 0;
        //     while (i < renderingResult.FiberVms.Count)
        //     {
        //         for (int j = 0; j < Portion; j++)
        //         {
        //             if (i >= renderingResult.FiberVms.Count) break;
        //             var fiberVm = renderingResult.FiberVms[i];
        //             var oldFiberVm = _graphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberVm.Id);
        //             if (oldFiberVm == null)
        //                 _graphReadModel.Data.Fibers.Add(fiberVm);
        //             else
        //             {
        //                 // though fiber exists already it's characteristics could be changed
        //                 if (oldFiberVm.TracesWithExceededLossCoeff.Any() ^ fiberVm.TracesWithExceededLossCoeff.Any()
        //                     || oldFiberVm.State != fiberVm.State)
        //                 {
        //                     _graphReadModel.Data.Fibers.Remove(oldFiberVm);
        //                     _graphReadModel.Data.Fibers.Add(fiberVm);
        //                 }
        //                 else
        //                 {
        //                     oldFiberVm.States = fiberVm.States;
        //                     oldFiberVm.TracesWithExceededLossCoeff = fiberVm.TracesWithExceededLossCoeff;
        //                 }
        //             }
        //             i++;
        //         }
        //         await Task.Delay(Delay);
        //     }
        // }

    //     private async Task RemoveElementsOfHiddenTraces(RenderingResult renderingResult)
    //     {
    //         // remove nodes for hidden traces
    //         var i = 0;
    //         var nodeVms = _graphReadModel.Data.Nodes.ToList();
    //         while (i < nodeVms.Count)
    //         {
    //             for (int j = 0; j < Portion; j++)
    //             {
    //                 if (i >= nodeVms.Count) break;
    //                 var nodeVm = nodeVms[i];
    //                 if (renderingResult.NodeVms.All(n => n.Id != nodeVm.Id))
    //                     _graphReadModel.Data.Nodes.Remove(nodeVm);
    //                 i++;
    //             }
    //
    //             await Task.Delay(Delay);
    //         }
    //
    //         // remove fibers for hidden traces
    //         i = 0;
    //         var fiberVms = _graphReadModel.Data.Fibers.ToList();
    //         while (i < fiberVms.Count)
    //         {
    //             for (int j = 0; j < Portion; j++)
    //             {
    //                 if (i >= fiberVms.Count) break;
    //                 var fiberVm = fiberVms[i];
    //                 if (renderingResult.FiberVms.All(f => f.Id != fiberVm.Id))
    //                     _graphReadModel.Data.Fibers.Remove(fiberVm);
    //                 i++;
    //             }
    //             await Task.Delay(Delay);
    //         }
    //     }
    }
}