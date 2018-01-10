using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class LandmarksGraphParser
    {
        private readonly ReadModel _readModel;

        public LandmarksGraphParser(ReadModel readModel)
        {
            _readModel = readModel;
        }

        public List<Landmark> GetLandmarks(Trace trace)
        {
            var list = trace.Nodes.Select((t, i) => CombineLandmark(t, trace.Equipments[i], i)).ToList();
            for (var i = 1; i < list.Count; i++)
                list[i].Location = list[i].GpsCoors.GetDistanceKm(list[i - 1].GpsCoors) + list[i - 1].Location;
            return list;
        }

        private Landmark CombineLandmark(Guid nodeId, Guid equipmentId, int number)
        {
            var node = _readModel.Nodes.First(n => n.Id == nodeId);
            var result = new Landmark()
            {
                Number = number,
                NodeTitle = node.Title,
                EventNumber = 0,
                GpsCoors = new PointLatLng(node.Latitude, node.Longitude)
            };

            if (number == 0)
            {
                var rtu = _readModel.Rtus.First(e => e.Id == equipmentId);
                result.EquipmentTitle = rtu.Title;
                result.EquipmentType = EquipmentType.Rtu;
            }
            else
            {
                if (equipmentId != Guid.Empty)
                {
                    var equipment = _readModel.Equipments.First(e => e.Id == equipmentId);
                    result.EquipmentTitle = equipment.Title;
                    result.EquipmentType = equipment.Type;
                }
                else result.EquipmentType = EquipmentType.EmptyNode;
            }
            return result;
        }

    }
}