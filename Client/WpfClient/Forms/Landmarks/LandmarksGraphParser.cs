﻿using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class LandmarksGraphParser
    {
        private readonly Model _readModel;

        public LandmarksGraphParser(Model readModel)
        {
            _readModel = readModel;
        }

        public List<Landmark> GetLandmarks(Trace trace)
        {
            var list = trace.NodeIds.Select((t, i) => CombineLandmark(t, trace.EquipmentIds[i], i)).ToList();
            for (var i = 1; i < list.Count; i++)
                list[i].Location = list[i].GpsCoors.GetDistanceKm(list[i - 1].GpsCoors) + list[i - 1].Location;
            return list;
        }

        private Landmark CombineLandmark(Guid nodeId, Guid equipmentId, int number)
        {
            var node = _readModel.Nodes.First(n => n.NodeId == nodeId);
            var result = new Landmark()
            {
                Number = number,
                NodeTitle = node.Title,
                EventNumber = 0,
                GpsCoors = node.Position,
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
                    var equipment = _readModel.Equipments.First(e => e.EquipmentId == equipmentId);
                    result.EquipmentTitle = equipment.Title;
                    result.EquipmentType = equipment.Type;
                }
                else result.EquipmentType = EquipmentType.EmptyNode;
            }
            return result;
        }

    }
}