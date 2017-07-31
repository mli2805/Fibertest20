using System;

namespace DataCenterCore
{
    public class ClientStation : ICloneable
    {
        public string Ip { get; set; }


        public object Clone()
        {
            return new ClientStation() {Ip = (string) Ip.Clone()};
        }
    }
}