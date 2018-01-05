using System.Windows;
using System.Windows.Media;

namespace Iit.Fibertest.Client
{
    public class ConfirmaionLineModel
    {
        public string Line { get; set; }
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        public Brush Foreground { get; set; } = Brushes.Black;
    }
}