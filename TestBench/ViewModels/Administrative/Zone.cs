using System;
using System.Collections.Generic;

namespace Iit.Fibertest.TestBench
{
    [Serializable]
    public class Zone : ICloneable
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public List<Guid> Objects { get; set; } = new List<Guid>();

        public override string ToString()
        {
            return Title;
        }

        public object Clone()
        {
            var result = (Zone)MemberwiseClone();
            result.Objects = new List<Guid>(Objects);
            return result;
        }
    }
}