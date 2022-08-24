using System.Windows;

namespace Iit.Fibertest.Client
{
    public class GponRelationInfo
    {
        public Visibility Visibility { get; set; } = Visibility.Visible;

        public string TceTitle { get; set; }
        public string TceType { get; set; }
        public int SlotPosition;
        public string SlotPositionStr => SlotPosition == 0 ? "" : SlotPosition.ToString();

        public int GponInterfaceNumber;
        public string GponInterfaceNumberStr => GponInterfaceNumber == 0 ? "" : GponInterfaceNumber.ToString();
    }
}
