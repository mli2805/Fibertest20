﻿using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class GraphVisibilityExt
    {
        public static string GetLocalizedString(this GraphVisibilityLevel level)
        {
            switch (level)
            {
                case GraphVisibilityLevel.OnlyRtu: return Resources.SID_Only_RTU;
                case GraphVisibilityLevel.RtuAndTraces: return Resources.SID_RTU_and_traces;
                case GraphVisibilityLevel.Equipments: return Resources.SID_Equip;
                case GraphVisibilityLevel.EmptyNodes: return Resources.SID_Nodes;
                case GraphVisibilityLevel.AllDetails: return Resources.SID_All;
                default: return Resources.SID_Unknown;
            }
        }

        public static List<GraphVisibilityLevelItem> GetComboboxItems()
        {
            return (from level in Enum.GetValues(typeof(GraphVisibilityLevel)).OfType<GraphVisibilityLevel>()
                    select new GraphVisibilityLevelItem(level)).ToList();
        }

        public static GraphVisibilityLevel GetEnabledVisibilityLevel(this EquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case EquipmentType.AdjustmentPoint: return GraphVisibilityLevel.AllDetails;
                case EquipmentType.EmptyNode: return GraphVisibilityLevel.EmptyNodes;
                case EquipmentType.CableReserve:
                case EquipmentType.Other:
                case EquipmentType.Closure:
                case EquipmentType.Cross:
                case EquipmentType.Well:
                case EquipmentType.Terminal: return GraphVisibilityLevel.Equipments;
                case EquipmentType.Rtu: return GraphVisibilityLevel.OnlyRtu;
                default: return GraphVisibilityLevel.AllDetails;
            }
        }
    }
}