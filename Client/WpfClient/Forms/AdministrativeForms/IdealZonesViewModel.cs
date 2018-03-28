using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class ObjectToZonesModel
    {
        public string ObjectTitle { get; set; }
        public Guid ObjectId { get; set; }
        public bool IsRtu { get; set; }

        public bool[] Zones { get; set; }


        public ObjectToZonesModel(int maxZonesCount)
        {
            Zones = new bool[maxZonesCount];
        }
    }




    public class IdealZonesViewModel : Screen
    {
        private int MaxZoneCount = 12;
        public Visibility[] ZonesVisibility { get; set; }

        public  ReadModel ReadModel { get; set; }
        public List<ObjectToZonesModel> Rows { get; set; } = new List<ObjectToZonesModel>();
        public ObjectToZonesModel SelectedRow { get; set; }

        public bool IsReadOnly { get; set; }

        public IdealZonesViewModel(ReadModel readModel, CurrentUser currentUser, IWcfServiceForClient c2DWcfManager)
        {
            ReadModel = readModel;
            IsReadOnly = currentUser.Role > Role.Root;
            Initialize();
        }

        private void Initialize()
        {
            ZonesVisibility = Enumerable.Repeat(Visibility.Collapsed, MaxZoneCount).ToArray();
            for (var index = 0; index < ReadModel.Zones.Count; index++)
                ZonesVisibility[index] = Visibility.Visible;

            foreach (var rtu in ReadModel.Rtus)
            {
                Rows.Add(RtuToLine(rtu));

                foreach (var trace in ReadModel.Traces.Where(t=>t.RtuId == rtu.Id))
                {
                    var traceLine = new ObjectToZonesModel(MaxZoneCount)
                    {
                        ObjectTitle = @"  " + trace.Title,
                        ObjectId = trace.Id,
                        IsRtu = false,
                    };
                    Rows.Add(traceLine);
                }
            }

           
        }

        private ObjectToZonesModel RtuToLine(Rtu rtu)
        {
            var rtuLine = new ObjectToZonesModel(MaxZoneCount)
            {
                ObjectTitle = rtu.Title,
                ObjectId = rtu.Id,
                IsRtu = true,
            };
            for (var i = 0; i < ReadModel.Zones.Count; i++)
            {
                var zone = ReadModel.Zones[i];
                rtuLine.Zones[i] = rtu.ZoneIds.Contains(zone.ZoneId);
            }

            return rtuLine;
        }

        public void OnClick(int columnIndex)
        {

        }
    }
}
