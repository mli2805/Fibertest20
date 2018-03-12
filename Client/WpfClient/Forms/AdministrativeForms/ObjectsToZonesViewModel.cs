using System.Data;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class ObjectsToZonesViewModel : Screen
    {
        private readonly ReadModel _readModel;
        public DataTable TableForBinding { get; set; }

        public ObjectsToZonesViewModel(ReadModel readModel)
        {
            _readModel = readModel;

            Initialize();
        }

        private void Initialize()
        {
            TableForBinding = new DataTable();
            TableForBinding.Columns.Add(new DataColumn("Objects"){DataType = typeof(string), Caption = @"Caption", ColumnName = "", ReadOnly = true});

            foreach (var zone in _readModel.Zones)
            {
                TableForBinding.Columns.Add(new DataColumn(zone.Title){DataType = typeof(bool), ReadOnly = zone.IsDefaultZone});
            }

            foreach (var rtu in _readModel.Rtus)
            {
                DataRow rtuRow = TableForBinding.NewRow();
                rtuRow[0] = rtu.Title;
//                rtuRow[1] = true;
                rtuRow[2] = false;
                rtuRow[3] = false;
                TableForBinding.Rows.Add(rtuRow);

                foreach (var trace in _readModel.Traces.Where(t=>t.RtuId == rtu.Id))
                {
                    DataRow traceRow = TableForBinding.NewRow();
                    traceRow[0] = @"   "+trace.Title;
//                    traceRow[1] = true;
                    traceRow[2] = false;
                    traceRow[3] = false;
                    TableForBinding.Rows.Add(traceRow);

                }
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones_settings;
        }

        public void OnCellChanged(int column, int row)
        {

        }

        public void Save() { TryClose(); }
        public void Cancel() { TryClose(); }
    }
}
