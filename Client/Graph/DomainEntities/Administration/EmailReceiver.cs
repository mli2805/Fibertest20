using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class EmailReceiver
    {
        public string Address { get; set; }
        public bool IsActivated { get;set; }
    }
}