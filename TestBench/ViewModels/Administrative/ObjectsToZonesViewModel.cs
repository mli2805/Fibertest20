using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class ObjectsToZonesViewModel : Screen
    {
        private readonly AdministrativeDb _administrativeDb;
        private readonly TreeOfRtuModel _treeOfRtuModel;

        public DataTable BindableTable { get; set; }
        public List<Guid> ObjectList { get; set; } = new List<Guid>();

        public ObjectsToZonesViewModel(AdministrativeDb administrativeDb, TreeOfRtuModel treeOfRtuModel)
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
                var rtuLeaf = (RtuLeaf) root;
                BindableTable.Rows.Add(CreateDataRow(rtuLeaf.Id, @" " + rtuLeaf.Title));

                foreach (var child in rtuLeaf.ChildrenImpresario.Children)
                {
                    var traceLeaf = child as TraceLeaf;
                    if (traceLeaf != null)
                        BindableTable.Rows.Add(CreateDataRow(traceLeaf.Id, @"   " + traceLeaf.Title));
                }
            }
        }

        private DataRow CreateDataRow(Guid id, string objectTitle)
        {
            DataRow newRow = BindableTable.NewRow();
            newRow["Objects"] = objectTitle;
            newRow[1] = true;

            int i = 2;
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

            DataColumn objectsColumn = new DataColumn() {ColumnName = "Objects"};
            BindableTable.Columns.Add(objectsColumn);

            foreach (var zone in _administrativeDb.Zones)
            {
                BindableTable.Columns.Add(new DataColumn(zone.Title) {DataType = typeof(bool)});
            }
        }

        public void Save()
        {
            foreach (var zone in _administrativeDb.Zones.Skip(1))
                zone.Objects = new List<Guid>();

            for (var r = 0; r < BindableTable.Rows.Count; r++)
            {
                for (var c = 2; c < BindableTable.Columns.Count; c++)
                {
                    if ((bool)BindableTable.Rows[r][c])
                    {
                        _administrativeDb.Zones[c-1].Objects.Add(ObjectList[r]);
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
