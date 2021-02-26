namespace Iit.Fibertest.Dto
{
    public class AccidentLineDto
    {
        public string Caption { get; set; }
     
        public int Number { get; set; }
        public FiberState AccidentSeriousness { get; set; }
        public string AccidentTypeLetter { get; set; }
        public AccidentPlace AccidentPlace { get; set; }

        public string TopLeft { get; set; }
        public string TopCenter { get; set; }
        public string TopRight { get; set; }
        public string Bottom0 { get; set; }
        public string Bottom1 { get; set; }
        public string Bottom2 { get; set; }
        public string Bottom3 { get; set; }
        public string Bottom4 { get; set; }
        public string PngPath { get; set; } 

        public GeoPoint Position { get; set; }
    }
}