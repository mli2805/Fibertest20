// ReSharper disable InconsistentNaming

using System;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    public class VeexOtau
    {
        public bool connected { get; set; }
        public string id { get; set; }
        public VeexOtauConnectionParameters connectionParameters { get; set; }
        public int inputPortCount { get; set; }
        public bool isFwdm { get; set; }
        public string model { get; set; }
        public int portCount { get; set; }
        public string protocol { get; set; }
        public string serialNumber { get; set; }
    }
}
