using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class RtuPartStateExt
    {
        public static string GetLocalizedString(this RtuPartState state)
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

        public static Brush GetBrush(this RtuPartState state)
        {
            switch (state)
            {
                case RtuPartState.Broken:
                    return Brushes.Red;
                case RtuPartState.NotSetYet:
                    return Brushes.Transparent;
                case RtuPartState.Ok:
                    return Brushes.Transparent;
                default:
                    return Brushes.Transparent;
            }

        }
    }
}