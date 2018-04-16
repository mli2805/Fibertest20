namespace Iit.Fibertest.Client
{
    public class RenderingManager
    {
        private readonly CurrentZoneRenderer _currentZoneRenderer;
        private readonly RenderingApplier _renderingApplier;

        public RenderingManager(CurrentZoneRenderer currentZoneRenderer,
             RenderingApplier renderingApplier)
        {
            _currentZoneRenderer = currentZoneRenderer;
            _renderingApplier = renderingApplier;
        }

        public void RenderCurrentZoneOnApplicationStart()
        {
            var renderingResult = _currentZoneRenderer.Do();
            _renderingApplier.ToEmptyGraph(renderingResult);
        }

        public void ReRenderCurrentZoneOnResponsibilitiesChanged()
        {
            var renderingResult = _currentZoneRenderer.Do();
            _renderingApplier.ToExistingGraph(renderingResult);
        }

        // Hide traces of RTU
        public void ReRenderCurrentZoneOnUsersHiddenTracesChanged()
        {
            var renderingResult = _currentZoneRenderer.Do();
            _renderingApplier.ToExistingGraph(renderingResult);
        }

        // Show traces of RTU

    }

  
}