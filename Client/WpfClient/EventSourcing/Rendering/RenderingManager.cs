namespace Iit.Fibertest.Client
{
    public class RenderingManager
    {
        private readonly CurrentZoneRenderer _currentZoneRenderer;
        private readonly RenderingApplier _renderingApplier;

        public RenderingManager(CurrentZoneRenderer currentZoneRenderer,
             RenderingApplier renderingApplier, CurrentlyHiddenRtu currentlyHiddenRtu)
        {
            _currentZoneRenderer = currentZoneRenderer;
            _renderingApplier = renderingApplier;

            currentlyHiddenRtu.Collection.CollectionChanged += HiddenRtu_CollectionChanged;
        }

        private void HiddenRtu_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ReRenderCurrentZoneOnUsersHiddenTracesChanged();
        }

        public void RenderCurrentZoneOnApplicationStart()
        {
            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToEmptyGraph(renderingResult);
        }

        public void ReRenderCurrentZoneOnResponsibilitiesChanged()
        {
            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToExistingGraph(renderingResult);
        }

        // Show-Hide traces of RTU
        public void ReRenderCurrentZoneOnUsersHiddenTracesChanged()
        {
            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToExistingGraph(renderingResult);
        }
    }
}