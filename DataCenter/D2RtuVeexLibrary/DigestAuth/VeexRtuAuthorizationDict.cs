using System.Collections.Generic;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class VeexRtuAuthorizationDict
    {
        // key is a http request host
        public Dictionary<string, VeexRtuAuthorizationData> Dict = new Dictionary<string, VeexRtuAuthorizationData>();
    }
}