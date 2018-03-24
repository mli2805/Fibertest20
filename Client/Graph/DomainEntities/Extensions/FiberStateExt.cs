using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Iit.Fibertest.Graph
{
    public static class FiberStateExt
    {
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
//                case FiberState.Unknown:
//                    return Resources.SID_Unknown;
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
//                    return isForeground ? Brushes.Purple : Brushes.LightPink;
                    return new SolidColorBrush(Color.FromArgb(255, 128, 128, 192));
                case FiberState.Major:
                    return isForeground ? Brushes.Fuchsia :  Brushes.HotPink;
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

        public static Uri GetPictogram(this FiberState state)
        {
            switch (state)
            {
                case FiberState.NotJoined:
                case FiberState.Unknown:
                    return new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png");
                case FiberState.Ok:
                    return new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png");
                case FiberState.Suspicion:
                    return new Uri("pack://application:,,,/Resources/LeftPanel/YellowSquare.png");
                case FiberState.Minor:
                    return new Uri("pack://application:,,,/Resources/LeftPanel/PurpleSquare.png");
                case FiberState.Major:
                    return new Uri("pack://application:,,,/Resources/LeftPanel/FuchsiaSquare.png");
                case FiberState.User:
                    return new Uri("pack://application:,,,/Resources/LeftPanel/GreenSquare.png");
                case FiberState.Critical:
                case FiberState.FiberBreak:
                case FiberState.NoFiber:
                    return new Uri("pack://application:,,,/Resources/LeftPanel/RedSquare.png");
                default:
                    return new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png");
            }
        }
    }
}