using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class ZonesViewModel : Screen
    {
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly UsersDb _usersDb;

        private ObservableCollection<ObjectToZoneBelongingLine> _rows;
        public ObservableCollection<ObjectToZoneBelongingLine> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<string> _zoneList;
        public ObservableCollection<string> ZoneList
        {
            get { return _zoneList; }
            set
            {
                if (Equals(value, _zoneList)) return;
                _zoneList = value;
                NotifyOfPropertyChange();
            }
        }

        public string NewZoneTitle { get; set; }

        public ZonesViewModel(TreeOfRtuModel treeOfRtuModel, UsersDb usersDb)
        {
            _treeOfRtuModel = treeOfRtuModel;
            _usersDb = usersDb;
            CompileTable();
        }

        private void CompileTable()
        {
            Rows = GetRtuTraceCollection();
            ZoneList = new ObservableCollection<string>(_usersDb.Zones.Skip(1).Select(z => z.Name));
            foreach (var line in Rows)
            {
                foreach (var zone in _usersDb.Zones.Skip(1))
                {
                    line.Belongings.Add(zone.Objects.Contains(line.ObjectId));
                }
            }
        }

        private ObservableCollection<ObjectToZoneBelongingLine> GetRtuTraceCollection()
        {
            var itemList = new ObservableCollection<ObjectToZoneBelongingLine>();
            foreach (var leaf in _treeOfRtuModel.Tree)
            {
                var rtuLeaf = (RtuLeaf) leaf;
                itemList.Add(new ObjectToZoneBelongingLine() {ObjectId = rtuLeaf.Id, ObjectTitle = @" " + rtuLeaf.Title.Trim()});
                foreach (var child in rtuLeaf.ChildrenImpresario.Children)
                {
                    var traceLeaf = child as TraceLeaf;
                    if (traceLeaf != null)
                        itemList.Add(new ObjectToZoneBelongingLine() {ObjectId = traceLeaf.Id, ObjectTitle = @"   " + traceLeaf.Title.Trim()});
                }
            }
            return itemList;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones;
        }

        public void AddZone()
        {
            var newZone = new Zone() {Id = Guid.NewGuid(), Name = NewZoneTitle };
            _usersDb.Zones.Add(newZone);
            ZoneList.Add(newZone.Name);
            foreach (var line in Rows)
            {
                line.Belongings.Add(false);
            }
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
