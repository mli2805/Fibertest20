namespace Iit.Fibertest.Client
{
    public class RenderingManager
    {
        private readonly CurrentZoneRenderer _currentZoneRenderer;
        private readonly RenderingApplier _renderingApplier;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;

        public RenderingManager(CurrentZoneRenderer currentZoneRenderer,
             RenderingApplier renderingApplier, CurrentlyHiddenRtu currentlyHiddenRtu)
        {
            _currentZoneRenderer = currentZoneRenderer;
            _renderingApplier = renderingApplier;
            _currentlyHiddenRtu = currentlyHiddenRtu;
        }

        private void HiddenRtu_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Show-Hide traces of RTU
            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToExistingGraph(renderingResult);
        }

        public void RenderCurrentZoneOnApplicationStart()
        {
            _currentlyHiddenRtu.Initialize();
            _currentlyHiddenRtu.PropertyChanged += _currentlyHiddenRtu_PropertyChanged;
            _currentlyHiddenRtu.Collection.CollectionChanged += HiddenRtu_CollectionChanged;
            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToEmptyGraph(renderingResult);
        }

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

    }
}