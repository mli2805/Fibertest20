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
          //  _currentlyHiddenRtu.Collection.CollectionChanged += HiddenRtu_CollectionChanged; // show/hide all graph
            _currentlyHiddenRtu.PropertyChanged += _currentlyHiddenRtu_PropertyChanged;
        }

        private void _currentlyHiddenRtu_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsShowAllPressed" || e.PropertyName == "IsHideAllPressed")
            {
                FullClean();
                ShowOrHideAllOnEmptyMap();
            }

            if (e.PropertyName == "ChangedRtu")
            {
                var renderingResult = _currentZoneRenderer.GetRendering();
                _renderingApplier.ToExistingGraph(renderingResult);
            }

            _currentlyHiddenRtu.CleanFlags();
        }

        public void RenderCurrentZoneOnApplicationStart()
        {
            ShowOrHideAllOnEmptyMap();
        }

     
        private void ShowOrHideAllOnEmptyMap()
        {
            if (_currentUser.Role <= Role.Root)
            {
                var renderingResult = _currentlyHiddenRtu.Collection.Count == 0
                    ? _rootRenderAndApply.ShowAll()
                    : _rootRenderAndApply.ShowOnlyRtusAndNotInTraces();
                _renderingApplier.ToEmptyGraph(renderingResult);
            }
            else
            {
                var renderingResult = _currentlyHiddenRtu.Collection.Count == 0
                    ? _lessThanRootRenderAndApply.ShowAllOnStart()
                    : _lessThanRootRenderAndApply.ShowOnlyRtus();
                _renderingApplier.ToEmptyGraph(renderingResult);
            }
        }

        public void ReRenderCurrentZoneOnResponsibilitiesChanged()
        {
            if (_currentUser.ZoneId == Guid.Empty) return; // it is a default zone user

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