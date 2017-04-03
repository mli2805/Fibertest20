using System;
using System.Data;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class ObjectsToZonesViewModel : Screen
    {
        private readonly UsersDb _usersDb;
        private readonly TreeOfRtuModel _treeOfRtuModel;

        public DataTable Source { get; set; }

        public ObjectsToZonesViewModel(UsersDb usersDb, TreeOfRtuModel treeOfRtuModel)
        {
            _usersDb = usersDb;
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
                Source.Rows.Add(CreateDataRow(rtuLeaf.Id, @" " + rtuLeaf.Title));

                foreach (var child in rtuLeaf.ChildrenImpresario.Children)
                {
                    var traceLeaf = child as TraceLeaf;
                    if (traceLeaf != null)
                        Source.Rows.Add(CreateDataRow(traceLeaf.Id, @"   " + traceLeaf.Title));
                }
            }
        }

        private DataRow CreateDataRow(Guid id, string objectTitle)
        {
            DataRow newRow = Source.NewRow();
            newRow["Objects"] = objectTitle;
            newRow[1] = true;

            int i = 2;
            foreach (var zone in _usersDb.Zones.Skip(1))
            {
                newRow[i] = zone.Objects.Contains(id);
                i++;
            }
            return newRow;
        }

        private void CreateTable()
        {
            Source = new DataTable();   

            DataColumn objectsColumn = new DataColumn() {ColumnName = "Objects"};
            Source.Columns.Add(objectsColumn);


            foreach (var zone in _usersDb.Zones)
            {
                var dataColumn = new DataColumn(zone.Title);
                dataColumn.DataType = typeof(bool);
                Source.Columns.Add(dataColumn);
            }
        }

        public void Save()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }

    }



}
