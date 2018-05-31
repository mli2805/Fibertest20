using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class RootRenderAndApply
    {
        private readonly Model _readModel;
        private readonly IMyLog _logFile;
        private readonly GraphReadModel _graphReadModel;

        public RootRenderAndApply(Model readModel, IMyLog logFile, GraphReadModel graphReadModel)
        {
            _readModel = readModel;
            _logFile = logFile;
            _graphReadModel = graphReadModel;
        }

        public void ShowAllOnStart()
        {
            foreach (var node in _readModel.Nodes)
                _graphReadModel.Data.Nodes.Add(ElementRenderer.Map(node));

            foreach (var fiber in _readModel.Fibers)
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, _graphReadModel.Data.Nodes);
                if (fiberVm != null)
                    _graphReadModel.Data.Fibers.Add(fiberVm);
            }
        }

        public void HideAllOnStart()
        {
            foreach (var node in _readModel.Nodes)
                if (_readModel.Rtus.Any(r=>r.NodeId == node.NodeId) ||
                    !_readModel.Traces.Any(t=>t.NodeIds.Contains(node.NodeId)))
                    _graphReadModel.Data.Nodes.Add(ElementRenderer.Map(node));

            foreach (var fiber in _readModel.Fibers.Where(f=>f.States.Count == 0))
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, _graphReadModel.Data.Nodes);
                if (fiberVm != null)
                    _graphReadModel.Data.Fibers.Add(fiberVm);
            }
        }

        public void ShowAllOnClick()
        {
            _graphReadModel.Data.Fibers.Clear();
            _graphReadModel.Data.Nodes.Clear();
            
            for (int i = _graphReadModel.MainMap.Markers.Count-1; i >= 0; i--)
            {
                _graphReadModel.MainMap.Markers.RemoveAt(i);
            }
            ShowAllOnStart();
        }

        public void HideAllOnClick()
        {
            _logFile.AppendLine($@"1 {DateTime.Now.ToString(@"HH-mm-ss-FFF")}");
            _graphReadModel.Data.Fibers.Clear();
            _graphReadModel.Data.Nodes.Clear();
            _logFile.AppendLine($@"2 {DateTime.Now.ToString(@"HH-mm-ss-FFF")}");
            for (int i = _graphReadModel.MainMap.Markers.Count - 1; i >= 0; i--)
            {
                _graphReadModel.MainMap.Markers.RemoveAt(i);
            }
            _logFile.AppendLine($@"3 {DateTime.Now.ToString(@"HH-mm-ss-FFF")}");
            HideAllOnStart();
            _logFile.AppendLine($@"4 {DateTime.Now.ToString(@"HH-mm-ss-FFF")}");
        }
    }
}