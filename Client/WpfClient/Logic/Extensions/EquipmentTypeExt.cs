using System;
using System.Windows.Media.Imaging;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class EquipmentTypeExt
    {
        public static BitmapImage GetPictogramBitmapImage(EquipmentType type, FiberState state)
        {
            string stateName = state.ToString();
            string typeName = type.ToString();
            var path = $@"pack://application:,,,/Resources/{typeName}/{typeName}{stateName}.png";
            return new BitmapImage(new Uri(path));
        }

        
        public static string ToLocalizedString(this EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.AdjustmentPoint:
                    return Resources.SID_Adjustment_point;
                case EquipmentType.EmptyNode:
                    return Resources.SID_Node_without_equipment;
                case EquipmentType.CableReserve:
                    return Resources.SID_CableReserve;
                case EquipmentType.Other:
                    return Resources.SID_Other;
                case EquipmentType.Closure:
                    return Resources.SID_Closure;
                case EquipmentType.Cross:
                    return Resources.SID_Cross;
                case EquipmentType.Well:
                    return Resources.SID_Well;
                case EquipmentType.Terminal:
                    return Resources.SID_Terminal;

                case EquipmentType.Rtu:
                    return Resources.SID_Rtu;
            }
            return Resources.SID_Switch_ended_unexpectedly;
        }
    }
}