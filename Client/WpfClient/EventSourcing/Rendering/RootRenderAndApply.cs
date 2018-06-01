using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class LessThanRootRenderAndApply
    {
        private readonly Model _readModel;
        private readonly IMyLog _logFile;
        private readonly GraphReadModel _graphReadModel;
        private readonly CurrentUser _currentUser;

        public LessThanRootRenderAndApply(Model readModel, IMyLog logFile, GraphReadModel graphReadModel, CurrentUser currentUser)
        {
            _readModel = readModel;
            _logFile = logFile;
            _graphReadModel = graphReadModel;
            _currentUser = currentUser;
        }

        public void ShowAllOnStart()
        {
            foreach (var trace in _readModel.Traces.Where(r => r.ZoneIds.Contains(_currentUser.ZoneId)))
            {

            }
        }
    }

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
                if (_readModel.Rtus.Any(r => r.NodeId == node.NodeId) ||
                    !_readModel.Traces.Any(t => t.NodeIds.Contains(node.NodeId)))
                    _graphReadModel.Data.Nodes.Add(ElementRenderer.Map(node));

            foreach (var fiber in _readModel.Fibers.Where(f => f.States.Count == 0))
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, _graphReadModel.Data.Nodes);
                if (fiberVm != null)
                    _graphReadModel.Data.Fibers.Add(fiberVm);
            }
        }

        public void ShowAllOnClick()
        {
            FullClean();
            ShowAllOnStart();
        }

        public void HideAllOnClick()
        {
            FullClean();
            HideAllOnStart();
        }

        private void FullClean()
        {
            _graphReadModel.Data.Fibers.Clear();
            _graphReadModel.Data.Nodes.Clear();
            var start = DateTime.Now;
            for (int i = _graphReadModel.MainMap.Markers.Count - 1; i >= 0; i--)
            {
                _graphReadModel.MainMap.Markers.RemoveAt(i);
            }
            _logFile.AppendLine($@"MainMap.Markers are cleaned in {DateTime.Now - start}");
        }
    }
}