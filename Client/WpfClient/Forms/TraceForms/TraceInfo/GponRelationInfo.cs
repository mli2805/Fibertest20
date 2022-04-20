using System.Windows;

namespace Iit.Fibertest.Client
{
    public class GponRelationInfo
    {
        public Visibility Visibility { get; set; } = Visibility.Collapsed;

        public string TceTitle { get; set; }
        public string TceType { get; set; }
        public int SlotPosition { get; set; }
        public int GponInterfaceNumber { get; set; }
    }
}
