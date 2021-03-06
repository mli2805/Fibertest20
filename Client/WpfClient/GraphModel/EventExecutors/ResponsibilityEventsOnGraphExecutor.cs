﻿using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class ResponsibilityEventsOnGraphExecutor
    {
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly RenderingManager _renderingManager;

        public ResponsibilityEventsOnGraphExecutor(CurrentUser currentUser, Model readModel,
            RenderingManager renderingManager)
        {
            _currentUser = currentUser;
            _readModel = readModel;
            _renderingManager = renderingManager;
        }

        public void ChangeResponsibilities(ResponsibilitiesChanged e)
        {
            if (_readModel.Zones.First(z => z.ZoneId == _currentUser.ZoneId).IsDefaultZone)
                return; // current user works with full graph

            foreach (var pair in e.ResponsibilitiesDictionary)
            {
                if (!pair.Value.Contains(_currentUser.ZoneId))
                    continue; // it's not current user's problem (other zones were changed)

                _renderingManager.ReRenderCurrentZoneOnResponsibilitiesChanged();
                return;
            }
        }
    }
}