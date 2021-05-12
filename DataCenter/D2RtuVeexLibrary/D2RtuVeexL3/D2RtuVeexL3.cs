using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        private readonly IMyLog _logFile;
        private readonly D2RtuVeexLayer2 _d2RtuVeexLayer2;

        public D2RtuVeexLayer3(IMyLog logFile, D2RtuVeexLayer2 d2RtuVeexLayer2)
        {
            _logFile = logFile;
            _d2RtuVeexLayer2 = d2RtuVeexLayer2;
        }
    }
}