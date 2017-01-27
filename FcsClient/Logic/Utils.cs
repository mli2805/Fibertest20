using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public static class Utils
    {
        public static BitmapImage GetPictogramBitmapImage(EquipmentType type, FiberState state)
        {
            string stateName = state.ToString();
            string typeName = type.ToString();
            var path = $"pack://application:,,,/Resources/{typeName}{stateName}.png";
            return new BitmapImage(new Uri(path));
        }

        public static Brush StateToBrush(FiberState state)
        {
            switch (state)
            {
                case FiberState.NotInTrace:
                    return Brushes.Aqua;
                case FiberState.NotJoined:
                case FiberState.DistanceMeasurement:
                    return Brushes.Blue;

                case FiberState.Ok:
                    return Brushes.Black;
                case FiberState.Suspect:
                    return Brushes.Yellow;
                case FiberState.Minor:
                    return Brushes.Purple;
                case FiberState.Major:
                    return Brushes.Fuchsia;
                case FiberState.User:
                case FiberState.HighLighted:
                    return Brushes.Lime;
                case FiberState.Critical:
                case FiberState.FiberBreak:
                case FiberState.NoFiber:
                    return Brushes.Red;
                default:
                    return Brushes.Black;
            }
        }
    }
}