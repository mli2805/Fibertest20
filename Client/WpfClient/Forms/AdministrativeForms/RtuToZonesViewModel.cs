using System.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class RtuToZonesViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public DataTable TableForBinding { get; set; }
        public bool IsReadOnly { get; set; }

        public RtuToZonesViewModel(ReadModel readModel, CurrentUser currentUser, IWcfServiceForClient c2DWcfManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            IsReadOnly = currentUser.Role > Role.Root;

            PrepareTable();
            FillInTable();
        }

        private void PrepareTable()
        {
            TableForBinding = new DataTable();
            TableForBinding.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "RTU", ReadOnly = true });

            foreach (var zone in _readModel.Zones)
            {
                TableForBinding.Columns.Add(new DataColumn(zone.Title)
                {
                    DataType = typeof(bool),
                    ReadOnly = zone.IsDefaultZone
                });
            }
        }

        private void FillInTable()
        {
            foreach (var rtu in _readModel.Rtus)
            {
                    DataRow rtuRow = TableForBinding.NewRow();
                    rtuRow[0] = rtu.Title;
                    for (int i = 1; i < _readModel.Zones.Count; i++)
                        rtuRow[i + 1] = rtu.ZoneIds.Contains(_readModel.Zones[i].ZoneId);

                    TableForBinding.Rows.Add(rtuRow);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones_settings;
        }



    }
}
