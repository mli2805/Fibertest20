namespace Iit.Fibertest.Client
{
    public class LandmarksCorrectionProgressLine
    {
        public string Text {get; set; }
        public string Color {get; set; }
        // public Brush Brush {get; set; }

        public LandmarksCorrectionProgressLine(string text, string color)
        {
            Text = text;
            Color = color;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}