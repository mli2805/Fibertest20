using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class LessThanRootRenderAndApply
    {
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly OneTraceRenderer _oneTraceRenderer;

        public LessThanRootRenderAndApply(Model readModel, 
            CurrentUser currentUser, OneTraceRenderer oneTraceRenderer)
        {
            _readModel = readModel;
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

        public RenderingResult ShowOnlyRtus() // HideAll()
        {
            var renderingResult = new RenderingResult();
            foreach (var rtu in _readModel.Rtus)
            {
                var node = _readModel.Nodes.First(n => n.NodeId == rtu.NodeId);
                renderingResult.NodeVms.Add(ElementRenderer.Map(node));
            }
            return renderingResult;
        }
    }
}