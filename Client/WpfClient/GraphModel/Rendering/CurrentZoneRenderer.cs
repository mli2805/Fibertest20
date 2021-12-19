using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class CurrentZoneRenderer
    {
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly RootRenderer _rootRenderer;
        private readonly LessThanRootRenderer _lessThanRootRenderer;
        private RenderingResult _renderingResult;

        public CurrentZoneRenderer(IMyLog logFile,
            CurrentUser currentUser, 
            RootRenderer rootRenderer, LessThanRootRenderer lessThanRootRenderer)
        {
            _logFile = logFile;
            _currentUser = currentUser;
            _rootRenderer = rootRenderer;
            _lessThanRootRenderer = lessThanRootRenderer;
        }

        public RenderingResult GetRenderingForHiddenAll()
        {
            return (_currentUser.Role <= Role.Root)
                ? _rootRenderer.ShowOnlyRtusAndNotInTraces()
                : _lessThanRootRenderer.ShowOnlyRtus();
        }
        public RenderingResult GetRenderingForShowAll()
        {
            return (_currentUser.Role <= Role.Root)
                ? _rootRenderer.ShowAll()
                : _lessThanRootRenderer.ShowRtusAndTraces();
        }

        public RenderingResult GetCurrentRendering()
        {
            _renderingResult = _currentUser.Role <= Role.Root
                ?  _rootRenderer.ShowAllMinusHiddenTraces()
                : _lessThanRootRenderer.ShowRtusAndTraces();

            _logFile.AppendLine($@"{_renderingResult.NodeVms.Count} nodes ready");
            _logFile.AppendLine($@"{_renderingResult.FiberVms.Count} fibers ready");
            _logFile.AppendLine(@"Rendering finished");

            return _renderingResult;
        }

    }
}