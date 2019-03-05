using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public class ComponentsReportViewModel : Screen
    {
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly ComponentsReportProvider _componentsReportProvider;
        public ComponentsReportModel Model { get; set; } = new ComponentsReportModel();

        public PdfDocument Report { get; set; }

        public ComponentsReportViewModel(Model readModel, CurrentUser currentUser,
            ComponentsReportProvider componentsReportProvider)
        {
            _readModel = readModel;
            _currentUser = currentUser;
            _componentsReportProvider = componentsReportProvider;
        }

        public void Initialize()
        {
            Model.IsZoneSelectionEnabled = _currentUser.ZoneId == Guid.Empty;
            Model.Zones = Model.IsZoneSelectionEnabled 
                ? _readModel.Zones 
                : new List<Zone>() {_readModel.Zones.First(z => z.ZoneId == _currentUser.ZoneId)};
            Model.SelectedZone = Model.Zones.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Monitoring_system_components;
            Report = null;
        }

        public void CreateReport()
        {
            Report = _componentsReportProvider.Create(Model);
            TryClose();
        }
    }
}
