using System;
using System.Windows.Media.Imaging;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Graph
{
    public static class EquipmentTypeExt
    {
        public static BitmapImage GetNodePictogram(EquipmentType type, FiberState state)
        {
            return type == EquipmentType.AccidentPlace ?
                GetAccidentPictogramImage(state) :
                GetOkNodePictogramBitmapImage(type);
        }

        private static BitmapImage GetOkNodePictogramBitmapImage(EquipmentType type)
        {
            string stateName = FiberState.Ok.ToString();
            string typeName = type.ToString();
            var path = $@"pack://application:,,,/Resources/{typeName}/{typeName}{stateName}.png";
            return new BitmapImage(new Uri(path));
        }

        private static BitmapImage GetAccidentPictogramImage(FiberState state)
        {
            string stateName;
            switch (state)
            {
                case FiberState.Minor: stateName = @"Minor"; break;
                case FiberState.Major: stateName = @"Major"; break;
                default: stateName = @"Critical"; break;
            }
            return new BitmapImage(new Uri($@"pack://application:,,,/Resources/OnMap/{stateName}AccidentPlace.png"));
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

        public static LandmarkCode ToLandmarkCode(this EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.EmptyNode:
                    return LandmarkCode.Manhole;
                case EquipmentType.Other:
                    return LandmarkCode.Other;
                case EquipmentType.Closure:
                    return LandmarkCode.Coupler;
                case EquipmentType.Cross:
                    return LandmarkCode.WiringCloset;
                case EquipmentType.Terminal:
                    return LandmarkCode.RemoteTerminal;

                case EquipmentType.Rtu:
                    return LandmarkCode.FiberDistributingFrame;
            }

            return LandmarkCode.Other;
        }
    }
}