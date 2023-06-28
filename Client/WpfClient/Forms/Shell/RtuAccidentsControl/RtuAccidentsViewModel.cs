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
        public ObservableCollection<RtuAccidentModel> Rows { get; set; } = new ObservableCollection<RtuAccidentModel>();

        public RtuAccidentsViewModel(Model readModel)
        {
            _readModel = readModel;
            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Id", ListSortDirection.Descending));
        }

        public void AddAccident(RtuAccident accident)
        {
            var row = new RtuAccidentModel().Build(accident, _readModel);
            Rows.Add(row);
        }

        public void RemoveOldAccidentIfExists(RtuAccident accident)
        {
            var row = accident.IsMeasurementProblem
                ? Rows.FirstOrDefault(r => r.TraceId == accident.TraceId)
                : Rows.FirstOrDefault(r => r.RtuId == accident.RtuId);
            if (row != null)
                Rows.Remove(row);
        }
    }
}
