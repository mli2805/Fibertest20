using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class RenderingManager
    {
        private readonly IMyLog _logFile;
        private readonly CurrentZoneRenderer _currentZoneRenderer;
        private readonly OneTraceRenderer _oneTraceRenderer;
        private readonly RenderingApplier _renderingApplier;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly CurrentUser _currentUser;
        private readonly GraphReadModel _graphReadModel;
        private readonly RootRenderAndApply _rootRenderAndApply;
        private readonly LessThanRootRenderAndApply _lessThanRootRenderAndApply;

        public RenderingManager(IMyLog logFile, CurrentZoneRenderer currentZoneRenderer, OneTraceRenderer oneTraceRenderer,
             RenderingApplier renderingApplier, CurrentlyHiddenRtu currentlyHiddenRtu,
            CurrentUser currentUser, GraphReadModel graphReadModel,
            RootRenderAndApply rootRenderAndApply, LessThanRootRenderAndApply lessThanRootRenderAndApply)
        {
            _logFile = logFile;
            _currentZoneRenderer = currentZoneRenderer;
            _oneTraceRenderer = oneTraceRenderer;
            _renderingApplier = renderingApplier;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _currentUser = currentUser;
            _graphReadModel = graphReadModel;
            _rootRenderAndApply = rootRenderAndApply;
            _lessThanRootRenderAndApply = lessThanRootRenderAndApply;
        }

        public void Initialize()
        {
            _currentlyHiddenRtu.Initialize();
            _currentlyHiddenRtu.Collection.CollectionChanged += HiddenRtu_CollectionChanged; // show/hide all graph
        }

        public void RenderCurrentZoneOnApplicationStart()
        {
            if (_currentUser.Role <= Role.Root)
            {
                if (_currentlyHiddenRtu.Collection.Count == 0)
                    _rootRenderAndApply.ShowAll();
                else
                    _rootRenderAndApply.HideAll();
            }
            else
            {
                if (_currentlyHiddenRtu.Collection.Count == 0)
                {
                    var renderingResult = _lessThanRootRenderAndApply.ShowAllOnStart();
                    _renderingApplier.ToEmptyGraph(renderingResult);
                }
                else
                    _lessThanRootRenderAndApply.HideAll();
            }
        }

        private void HiddenRtu_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_currentlyHiddenRtu.IsShowAllPressed)
            {
                _currentlyHiddenRtu.IsShowAllPressed = false;
                FullClean();
                if (_currentUser.Role <= Role.Root)
                {
                    _rootRenderAndApply.ShowAll();
                }
                else
                {
                    var renderingResult1 = _lessThanRootRenderAndApply.ShowAllOnStart();
                    _renderingApplier.ToEmptyGraph(renderingResult1);
                }
                return;
            }

            if (_currentlyHiddenRtu.IsHideAllPressed)
            {
                _currentlyHiddenRtu.IsHideAllPressed = false;
                FullClean();
                if (_currentUser.Role <= Role.Root)
                    _rootRenderAndApply.HideAll();
                else
                    _lessThanRootRenderAndApply.HideAll();
                return;
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

        // User asks to show broken or highlight trace
        public void ShowOneTrace(Trace trace)
        {
            var renderingResult = new RenderingResult();
            _oneTraceRenderer.GetRendering(trace, renderingResult);
            _renderingApplier.AddElementsOfShownTraces(renderingResult);
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