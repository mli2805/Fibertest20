using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class LessThanRootRenderer
    {
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly OneRtuOrTraceRenderer _oneRtuOrTraceRenderer;

        public LessThanRootRenderer(Model readModel, 
            CurrentUser currentUser, OneRtuOrTraceRenderer oneRtuOrTraceRenderer)
        {
            _readModel = readModel;
            _currentUser = currentUser;
            _oneRtuOrTraceRenderer = oneRtuOrTraceRenderer;
        }

        public RenderingResult ShowAllOnStart()
        {
            var renderingResult = ShowOnlyRtus();
            foreach (var trace in _readModel.Traces.Where(r => r.ZoneIds.Contains(_currentUser.ZoneId)))
            {
               _oneRtuOrTraceRenderer.GetTraceRendering(trace, renderingResult);
            }
            return renderingResult;
        }

        public RenderingResult ShowOnlyRtus() // HideAll()
        {
            var renderingResult = new RenderingResult();
            foreach (var rtu in _readModel.Rtus.Where(r=>r.ZoneIds.Contains(_currentUser.ZoneId)))
            {
                var node = _readModel.Nodes.First(n => n.NodeId == rtu.NodeId);
                renderingResult.NodeVms.Add(ElementRenderer.Map(node));
            }
            return renderingResult;
        }
    }
}