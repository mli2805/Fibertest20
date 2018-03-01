using System;

namespace Iit.Fibertest.Client
{
    public class AccidentLineVm
    {
        public string Caption { get; set; }
        public string TopLeft { get; set; }
        public string TopCenter { get; set; }
        public string TopRight { get; set; }
        public string Bottom0 { get; set; }
        public string Bottom1 { get; set; }
        public string Bottom2 { get; set; }
        public string Bottom3 { get; set; }
        public string Bottom4 { get; set; }
        public Uri Scheme => new Uri("pack://application:,,,/Resources/AccidentSchemes/AccidentBetweenNodes.png");
    }
}