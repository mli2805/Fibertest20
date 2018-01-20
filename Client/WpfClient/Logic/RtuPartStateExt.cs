using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class RtuPartStateExt
    {
        public static string ToLocalizedString(this RtuPartState state)
        {
            switch (state)
            {
                case RtuPartState.Broken:
                    return Resources.SID_Broken;
                case RtuPartState.NotSetYet:
                    return "";
                case RtuPartState.Ok:
                    return Resources.SID_Ok;
                default: return "";
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

        public static WorseOrBetter BecomeBetterOrWorse(this RtuPartState before, RtuPartState now)
        {
            return before < now ? WorseOrBetter.Better : before == now ? WorseOrBetter.TheSame : WorseOrBetter.Worse;
        }

        public static string GetPathToPictogram(this RtuPartState state)
        {
            switch (state)
            {
                case RtuPartState.Broken:
                    return @"pack://application:,,,/Resources/LeftPanel/RedSquare.png";
                case RtuPartState.NotSetYet:
                    return @"pack://application:,,,/Resources/LeftPanel/EmptySquare.png";
                case RtuPartState.Ok:
                    return @"pack://application:,,,/Resources/LeftPanel/GreenSquare.png";
                default:
                    return null;
            }
        }
    }
}