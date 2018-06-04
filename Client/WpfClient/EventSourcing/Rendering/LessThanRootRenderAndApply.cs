using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class LessThanRootRenderAndApply
    {
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly CurrentUser _currentUser;
        private readonly OneTraceRenderer _oneTraceRenderer;

        public LessThanRootRenderAndApply(Model readModel, GraphReadModel graphReadModel, 
            CurrentUser currentUser, OneTraceRenderer oneTraceRenderer)
        {
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _currentUser = currentUser;
            _oneTraceRenderer = oneTraceRenderer;
        }

        public RenderingResult ShowAllOnStart()
        {
            var renderingResult = new RenderingResult();
            foreach (var trace in _readModel.Traces.Where(r => r.ZoneIds.Contains(_currentUser.ZoneId)))
            {
               _oneTraceRenderer.GetRendering(trace, renderingResult);
            }

            return renderingResult;
        }

        public void HideAll()
        {
            foreach (var rtu in _readModel.Rtus)
            {
                var node = _readModel.Nodes.First(n => n.NodeId == rtu.NodeId);
                _graphReadModel.Data.Nodes.Add(ElementRenderer.Map(node));
            }
        }
    }
}