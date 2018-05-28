using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RenderingManager
    {
        private readonly CurrentZoneRenderer _currentZoneRenderer;
        private readonly OneTraceRenderer _oneTraceRenderer;
        private readonly RenderingApplier _renderingApplier;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;

        public RenderingManager(CurrentZoneRenderer currentZoneRenderer, OneTraceRenderer oneTraceRenderer,
             RenderingApplier renderingApplier, CurrentlyHiddenRtu currentlyHiddenRtu)
        {
            _currentZoneRenderer = currentZoneRenderer;
            _oneTraceRenderer = oneTraceRenderer;
            _renderingApplier = renderingApplier;
            _currentlyHiddenRtu = currentlyHiddenRtu;
        }

        public void RenderCurrentZoneOnApplicationStart()
        {
            _currentlyHiddenRtu.Initialize();
            _currentlyHiddenRtu.PropertyChanged += _currentlyHiddenRtu_PropertyChanged; // show/hide one RTU traces
            _currentlyHiddenRtu.Collection.CollectionChanged += HiddenRtu_CollectionChanged; // show/hide all graph
            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToEmptyGraph(renderingResult);
        }

        // show/hide all graph
        private void HiddenRtu_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Show-Hide traces of RTU
            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToExistingGraph(renderingResult);
        }

        // show/hide one RTU traces
        private void _currentlyHiddenRtu_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Show-Hide traces of RTU
            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToExistingGraph(renderingResult);
        }

        public void ReRenderCurrentZoneOnResponsibilitiesChanged()
        {
            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToExistingGraph(renderingResult);
        }

        public void ShowBrokenTrace(Trace trace)
        {
            var renderingResult = new RenderingResult();
            _oneTraceRenderer.GetRendering(trace, renderingResult);
            _renderingApplier.AddElementsOfShownTraces(renderingResult);
        }

    }
}