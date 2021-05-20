using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        private readonly IMyLog _logFile;
        private readonly D2RtuVeexLayer1 _d2RtuVeexLayer1;

        public D2RtuVeexLayer2(IMyLog logFile, D2RtuVeexLayer1 d2RtuVeexLayer1)
        {
            _logFile = logFile;
            _d2RtuVeexLayer1 = d2RtuVeexLayer1;
        }
    }
}