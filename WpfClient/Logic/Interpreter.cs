﻿using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public static Brush GetBrush(this FiberState state)
        {
            switch (state)
            {
                case FiberState.NotInTrace:
                    return Brushes.Aqua;
                case FiberState.NotJoined:
                case FiberState.DistanceMeasurement:
                    return Brushes.Blue;

                case FiberState.NotChecked:
                case FiberState.Ok:
                    return Brushes.Black;
                case FiberState.Suspect:
                    return Brushes.Yellow;
                case FiberState.Minor:
                    return Brushes.Purple;
                case FiberState.Major:
                    return Brushes.Fuchsia;
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
                case FiberState.NotChecked:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                case FiberState.Ok:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/BlackSquare.png"));
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
                case EquipmentType.Well:
                    return Resources.SID_Well;
                case EquipmentType.Invisible:
                    return Resources.SID_Adjustment_node;
                case EquipmentType.Closure:
                    return Resources.SID_Closure;
                case EquipmentType.Cross:
                    return Resources.SID_Cross;
                case EquipmentType.Terminal:
                    return Resources.SID_Terminal;
                case EquipmentType.Other:
                    return Resources.SID_Other;
                case EquipmentType.CableReserve:
                    return Resources.SID_CableReserve;
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
                case RtuPartState.None:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                case RtuPartState.Normal:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png"));
                default:
                    return null;
            }
        }

        public static ImageSource GetPictogram(this MonitoringState state)
        {
            switch (state)
            {
                case MonitoringState.Off:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
                case MonitoringState.OffButReady:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreySquare.png"));
                case MonitoringState.On:
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/BlueSquare.png"));
                default:
                    return null;
            }
        }
    }
}