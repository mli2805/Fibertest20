using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class Db
    {
        public List<object> Events { get; } = new List<object>();

        public void Add(object e)
        {
            Events.Add(e);
        }
    }
}