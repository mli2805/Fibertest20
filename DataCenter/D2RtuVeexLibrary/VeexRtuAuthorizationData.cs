using System;
using System.Collections.Generic;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class VeexRtuAuthorizationData
    {
        public Guid RtuId;
        public string Serial;

        public bool IsAuthorizationOn;

        public Dictionary<string, string> AuthenticationHeaderParts = new Dictionary<string, string>();

        public int Nc;

        public string Password => $"*{Serial.Substring(Serial.Length - 5)}~";
    }
}