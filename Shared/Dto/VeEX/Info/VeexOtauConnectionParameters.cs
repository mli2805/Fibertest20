using System;
// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Dto
{
    [Serializable]
    public class VeexOtauConnectionParameters
    {
        public string address { get; set; }
        public int port { get; set; }
        public string protocol { get; set; }
    }
}