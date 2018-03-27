using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class ObjectsToZonesViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        public DataTable TableForBinding { get; set; }
        public bool IsReadOnly { get; set; }

        private List<object> _subjects;
        private List<Guid> _subjectIds;


        public ObjectsToZonesViewModel(ReadModel readModel, CurrentUser currentUser, IWcfServiceForClient c2DWcfManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            IsReadOnly = currentUser.Role > Role.Root;

            PrepareTableAndLists();
            FillInTableAndLists();
        }

        private void PrepareTableAndLists()
        {
            _subjects = new List<object>();
            _subjectIds = new List<Guid>();

            TableForBinding = new DataTable();
            TableForBinding.Columns.Add(new DataColumn {DataType = typeof(string), ColumnName = "RTU", ReadOnly = true});
            TableForBinding.Columns.Add(new DataColumn {DataType = typeof(string), ColumnName = "Trace", ReadOnly = true});

            foreach (var zone in _readModel.Zones)
            {
                TableForBinding.Columns.Add(new DataColumn(zone.Title)
                {
                    DataType = typeof(bool),
                    ReadOnly = zone.IsDefaultZone
                });
            }
        }

        private void FillInTableAndLists()
        {
            foreach (var rtu in _readModel.Rtus)
            {
                var flag = true;

                foreach (var trace in _readModel.Traces.Where(t => t.RtuId == rtu.Id && t.OtauPort != null))
                {
                    DataRow traceRow = TableForBinding.NewRow();
                    traceRow[0] = flag ? rtu.Title : "";
                    traceRow[1] = trace.Title;
                    for (int i = 1; i < _readModel.Zones.Count; i++)
                        traceRow[i + 2] = trace.ZoneIds.Contains(_readModel.Zones[i].ZoneId);

                    TableForBinding.Rows.Add(traceRow);
                    _subjects.Add(trace);
                    _subjectIds.Add(trace.Id);

                    flag = false;
                }
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones_settings;
        }

     
        /// <summary>
        /// Command contains only CHANGES in responsibilities
        /// </summary>
        /// <returns></returns>
        private ChangeResponsibilities PrepareCommand()
        {
            var command = new ChangeResponsibilities(){ResponsibilitiesDictionary = new Dictionary<Guid, List<Guid>>()};
            for (var i = 0; i < TableForBinding.Rows.Count; i++)
            {
                var subjectId = _subjectIds[i];
                var oldListOfZones = GetOldListOfZones(i);

                var subjectZones = GetChangedZonesForSubject(i, oldListOfZones);

                command.ResponsibilitiesDictionary.Add(subjectId, subjectZones);
            }

            return command;
        }

        private List<Guid> GetChangedZonesForSubject(int row, List<Guid> oldListOfZones)
        {
            var changedZones = new List<Guid>();
            for (int j = 1; j < _readModel.Zones.Count; j++)
            {
                var value = TableForBinding.Rows[row][j+2];
                var isChecked = (bool) value;
                var zoneId = _readModel.Zones[j].ZoneId;

                if (isChecked ^ oldListOfZones.Contains(zoneId)) // put in list only if value is changed 
                    changedZones.Add(zoneId);
            }

            return changedZones;
        }

        private List<Guid> GetOldListOfZones(int i)
        {
            switch (_subjects[i])
            {
                case Rtu rtu: return rtu.ZoneIds;
                case Trace trace: return trace.ZoneIds; 
                default: return new List<Guid>(); 
            }
        }

        public async void Save()
        {
            if (!IsReadOnly)
                await _c2DWcfManager.SendCommandAsObj(PrepareCommand());

            TryClose();
        }
        public void Cancel() { TryClose(); }
    }
}
