using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RtuAccidentsViewModel : Screen
    {
        private readonly Model _readModel;
        public string TableTitle { get; set; }
        public ObservableCollection<RtuAccidentLineModel> Rows { get; set; } = new ObservableCollection<RtuAccidentLineModel>();

        public RtuAccidentsViewModel(Model readModel)
        {
            _readModel = readModel;
            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Accident.Id", ListSortDirection.Descending));
        }

        public void AddAccident(RtuAccident accident)
        {
            var row = new RtuAccidentLineModel(accident).Build(_readModel);
            Rows.Add(row);
        }

        public void RemoveOldAccidentIfExists(RtuAccident accident)
        {
            var row = accident.IsMeasurementProblem
                ? Rows.FirstOrDefault(r => r.Accident.TraceId == accident.TraceId)
                : Rows.FirstOrDefault(r => r.Accident.RtuId == accident.RtuId);
            if (row != null)
                Rows.Remove(row);
        }

        public void RefreshRowsWithUpdatedRtu(Guid rtuId)
        {
            foreach (var rtuAccidentLineModel in Rows.Where(m => m.Accident.RtuId == rtuId).ToList())
            {
                Rows.Remove(rtuAccidentLineModel);
                rtuAccidentLineModel.RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuId)?.Title;
                Rows.Add(rtuAccidentLineModel);
            }
        }

        public void RefreshRowsWithUpdatedTrace(Guid traceId)
        {
            foreach (var rtuAccidentLineModel in Rows.Where(m => m.Accident.TraceId == traceId).ToList())
            {
                Rows.Remove(rtuAccidentLineModel);
                rtuAccidentLineModel.TraceTitle = _readModel.Traces.FirstOrDefault(t => t.TraceId == traceId)?.Title;
                Rows.Add(rtuAccidentLineModel);
            }
        }

        public void RemoveAllEventsForRtu(Guid rtuId)
        {
            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                if (Rows[i].Accident.RtuId == rtuId)
                    Rows.RemoveAt(i);
            }
        }
        public void RemoveAllEventsForTrace(Guid traceId)
        {
            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                if (Rows[i].Accident.TraceId == traceId)
                    Rows.RemoveAt(i);
            }
        }

    }
}
