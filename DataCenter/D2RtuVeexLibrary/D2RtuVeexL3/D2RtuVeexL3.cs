namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        private readonly D2RtuVeexLayer2 _d2RtuVeexLayer2;
        private readonly VeexRtuAuthorizationDict _veexRtuAuthorizationDict;

        public D2RtuVeexLayer3(D2RtuVeexLayer2 d2RtuVeexLayer2, VeexRtuAuthorizationDict veexRtuAuthorizationDict)
        {
            _d2RtuVeexLayer2 = d2RtuVeexLayer2;
            _veexRtuAuthorizationDict = veexRtuAuthorizationDict;
        }
    }
}