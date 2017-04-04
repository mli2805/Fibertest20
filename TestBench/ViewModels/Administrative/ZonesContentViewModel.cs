using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class ZonesContentViewModel : Screen
    {
        private readonly AdministrativeDb _administrativeDb;
        private readonly TreeOfRtuModel _treeOfRtuModel;

        public DataTable BindableTable { get; set; }
        public List<Guid> ObjectList { get; set; } = new List<Guid>();

        public ZonesContentViewModel(AdministrativeDb administrativeDb, TreeOfRtuModel treeOfRtuModel)
        {
            _administrativeDb = administrativeDb;
            _treeOfRtuModel = treeOfRtuModel;

            CreateTable();
            PopulateTable();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones;
        }

        private void PopulateTable()
        {
            foreach (var root in _treeOfRtuModel.Tree)
            {
                var rtuTitle = ((RtuLeaf)root).Title;

                foreach (var child in ((RtuLeaf)root).ChildrenImpresario.Children)
                {
                    var traceLeaf = child as TraceLeaf;
                    if (traceLeaf != null)
                        BindableTable.Rows.Add(CreateDataRow(traceLeaf.Id, rtuTitle, traceLeaf.Title));

                    rtuTitle = "";
                }
            }
        }

        private DataRow CreateDataRow(Guid id, string rtuTitle, string traceTitle)
        {
            DataRow newRow = BindableTable.NewRow();
            newRow[0] = rtuTitle;
            newRow[1] = traceTitle;
            newRow[2] = true;

            int i = 3;
            foreach (var zone in _administrativeDb.Zones.Skip(1))
            {
                newRow[i] = zone.Objects.Contains(id);
                i++;
            }

            ObjectList.Add(id);
            return newRow;
        }

        private void CreateTable()
        {
            BindableTable = new DataTable();

            BindableTable.Columns.Add(new DataColumn() { ColumnName = Resources.SID_Rtu, ReadOnly = true});
            BindableTable.Columns.Add(new DataColumn() { ColumnName = Resources.SID_Traces, ReadOnly = true});

            foreach (var zone in _administrativeDb.Zones)
            {
                BindableTable.Columns.Add(new DataColumn(zone.Title) { DataType = typeof(bool) });
            }
        }

        public void Save()
        {
            foreach (var zone in _administrativeDb.Zones.Skip(1))
                zone.Objects = new List<Guid>();

            for (var r = 0; r < BindableTable.Rows.Count; r++)
            {
                for (var c = 3; c < BindableTable.Columns.Count; c++)
                {
                    if ((bool)BindableTable.Rows[r][c])
                    {
                        _administrativeDb.Zones[c - 2].Objects.Add(ObjectList[r]);
                    }
                }
            }
            _administrativeDb.Save();
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
