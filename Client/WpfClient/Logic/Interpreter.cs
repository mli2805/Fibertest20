using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class Interpreter
    {
        public static BitmapImage GetPictogramBitmapImage(EquipmentType type, FiberState state)
        {
            string stateName = state.ToString();
            string typeName = type.ToString();
            var path = $@"pack://application:,,,/Resources/{typeName}/{typeName}{stateName}.png";
            return new BitmapImage(new Uri(path));
        }

        public static string ToLocalizedString(this FiberState state)
        {
            switch (state)
            {
                case FiberState.NotInTrace:
                    return Resources.SID_Not_in_trace;
                case FiberState.NotJoined:
                    return Resources.SID_Not_joined;
                case FiberState.DistanceMeasurement:
                    return Resources.SID_Distace_measurement;
                case FiberState.Unknown:
                    return Resources.SID_Unknown;
                case FiberState.Ok:
                    return Resources.SID_Ok;
                case FiberState.Suspicion:
                    return Resources.SID_Suspicion;
                case FiberState.Minor:
                    return Resources.SID_Minor;
                case FiberState.Major:
                    return Resources.SID_Major;
                case FiberState.User:
                    return Resources.SID_User_s_threshold;
                case FiberState.Critical:
                    return Resources.SID_Critical;
                case FiberState.FiberBreak:
                    return Resources.SID_fiber_break;
                case FiberState.NoFiber:
                    return Resources.SID_No_fiber;
                case FiberState.HighLighted:
                    return Resources.SID_Highlighted;
                default:
                    return Resources.SID_Ok;
            }
        }

        public static Brush GetBrush(this FiberState state, bool isForeground)
        {
            switch (state)
            {
                case FiberState.NotInTrace:
                    return Brushes.Aqua;
                case FiberState.NotJoined:
                case FiberState.DistanceMeasurement:
                    return Brushes.Blue;

                case FiberState.Unknown:
                case FiberState.Ok:
                    return isForeground ? Brushes.Black : Brushes.Transparent;
                case FiberState.Suspicion:
                    return Brushes.Yellow;
                case FiberState.Minor:
                    return Brushes.LightPink;
                case FiberState.Major:
                    return Brushes.HotPink;
                case FiberState.User:
                    return Brushes.Green;
                case FiberState.Critical:
                case FiberState.FiberBreak:
                case FiberState.NoFiber:
                    return Brushes.Red;
                case FiberState.HighLighted:
                    return Brushes.Lime;
                default:
                    return Brushes.Black;
            }
        }

        public static ImageSource GetPictogram(this FiberState state)
        {
            switch (state)
            {
                case FiberState.NotJoined:
                case FiberState.Unknown:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                case FiberState.Ok:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png"));
                case FiberState.Suspicion:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/YellowSquare.png"));
                case FiberState.Minor:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/PurpleSquare.png"));
                case FiberState.Major:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/FuchsiaSquare.png"));
                case FiberState.User:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png"));
                case FiberState.Critical:
                case FiberState.FiberBreak:
                case FiberState.NoFiber:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/RedSquare.png"));
                default:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
            }
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

        public static ImageSource GetPictogram(this RtuPartState state)
        {
            switch (state)
            {
                case RtuPartState.Broken:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/RedSquare.png"));
                case RtuPartState.NotSetYet:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                case RtuPartState.Ok:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png"));
                default:
                    return null;
            }
        }

        public static string ToLocalizedString(this RtuPartState state)
        {
            switch (state)
            {
                  case  RtuPartState.Broken: return Resources.SID_Broken;
                  case  RtuPartState.NotSetYet: return "";
                  case  RtuPartState.Ok: return @"OK";
                default: return null;
            }
        }

        public static Brush GetBrush(this RtuPartState state, bool isForeground)
        {
            switch (state)
            {
                case RtuPartState.Broken:
                    return Brushes.Red;
                case RtuPartState.NotSetYet:
                    return isForeground ? Brushes.LightGray : Brushes.Transparent;
                case RtuPartState.Ok:
                    return Brushes.Black;
                default:
                    return Brushes.Black;
            }
        }
    }
}