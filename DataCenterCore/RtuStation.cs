using System;

namespace DataCenterCore
{
    public class RtuStation
    {
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public DateTime LastConnection { get; set; }
    }
}