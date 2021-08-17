using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class OltAdded
    {
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public OltModel OltModel { get; set; }
    }
}
