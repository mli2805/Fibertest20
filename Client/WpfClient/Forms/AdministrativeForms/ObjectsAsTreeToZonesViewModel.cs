using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class ObjectsAsTreeToZonesViewModel : Screen
    {
        public ReadModel ReadModel { get; }
        public List<ObjectToZonesModel> Rows { get; set; } = new List<ObjectToZonesModel>();
        public ObjectToZonesModel SelectedRow { get; set; }

        public ObjectsAsTreeToZonesViewModel(ReadModel readModel)
        {
            ReadModel = readModel;

            FillInRows();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones_subjects;
        }

        private void FillInRows()
        {
            foreach (var rtu in ReadModel.Rtus)
            {
                Rows.Add(RtuToLine(rtu));
                foreach (var trace in ReadModel.Traces.Where(t => t.RtuId == rtu.Id))
                    Rows.Add(TraceToLine(trace));
            }
        }

        private ObjectToZonesModel RtuToLine(Rtu rtu)
        {
            var rtuLine = new ObjectToZonesModel()
            {
                SubjectTitle = rtu.Title,
                RtuId = rtu.Id,
                IsRtu = true,
            };
            foreach (var zone in ReadModel.Zones)
                rtuLine.IsInZones.Add(new BoolWithNotification(){IsChecked = rtu.ZoneIds.Contains(zone.ZoneId)});
            return rtuLine;
        }

        private ObjectToZonesModel TraceToLine(Trace trace)
        {
            var traceLine = new ObjectToZonesModel()
            {
                SubjectTitle = @"  " + trace.Title,
                TraceId = trace.Id,
                RtuId = trace.RtuId,
                IsRtu = false,
            };
            foreach (var zone in ReadModel.Zones)
                traceLine.IsInZones.Add(new BoolWithNotification() { IsChecked = trace.ZoneIds.Contains(zone.ZoneId)});

            return traceLine;
        }

        public void Save()
        {
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
