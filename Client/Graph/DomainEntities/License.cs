using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class License
    {
        public string Owner { get; set; }
        public int RtuCount { get; set; }
        public int ClientStationCount { get; set; }
        public bool SuperClientEnabled { get; set; }
        public string Version { get; set; } = @"2.0.0.0";
    }
}
