using System;
using System.Collections.Generic;

namespace Iit.Fibertest.TestBench
{
    [Serializable]
    public class Zone
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Guid> Objects { get; set; }
    }
}