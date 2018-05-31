using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RenderingManager
    {
        private readonly CurrentZoneRenderer _currentZoneRenderer;
        private readonly OneTraceRenderer _oneTraceRenderer;
        private readonly RenderingApplier _renderingApplier;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly CurrentUser _currentUser;
        private readonly RootRenderAndApply _rootRenderAndApply;

        public RenderingManager(CurrentZoneRenderer currentZoneRenderer, OneTraceRenderer oneTraceRenderer,
             RenderingApplier renderingApplier, CurrentlyHiddenRtu currentlyHiddenRtu,
            CurrentUser currentUser, RootRenderAndApply rootRenderAndApply)
        {
            _currentZoneRenderer = currentZoneRenderer;
            _oneTraceRenderer = oneTraceRenderer;
            _renderingApplier = renderingApplier;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _currentUser = currentUser;
            _rootRenderAndApply = rootRenderAndApply;
        }

        public void RenderCurrentZoneOnApplicationStart()
        {
            _currentlyHiddenRtu.Initialize();
            _currentlyHiddenRtu.Collection.CollectionChanged += HiddenRtu_CollectionChanged; // show/hide all graph

            if (_currentUser.Role <= Role.Root)
            {
                if (_currentlyHiddenRtu.Collection.Count == 0)
                    _rootRenderAndApply.ShowAllOnStart();
                else
                    _rootRenderAndApply.HideAllOnStart();
            }
            else
            {
                var renderingResult = _currentZoneRenderer.GetRendering();
                _renderingApplier.ToEmptyGraph(renderingResult);
            }

        }

        private void HiddenRtu_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_currentUser.Role <= Role.Root)
            {
                if (_currentlyHiddenRtu.IsShowAllPressed)
                {
                    _currentlyHiddenRtu.IsShowAllPressed = false;
                    _rootRenderAndApply.ShowAllOnClick();
                    return;
                }

                if (_currentlyHiddenRtu.IsHideAllPressed)
                {
                    _currentlyHiddenRtu.IsHideAllPressed = false;
                    _rootRenderAndApply.HideAllOnClick();
                    return;
                }

            }

            // not All or not Root
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