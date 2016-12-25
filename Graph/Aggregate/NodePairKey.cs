using System;

namespace Iit.Fibertest.Graph
{
    public struct NodePairKey
    {
        private readonly Guid _a;
        private readonly Guid _b;

        public NodePairKey(Guid a, Guid b)
        {
            if (string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal) < 0)
            {
                _a = b;
                _b = a;
            }
            else
            {
                _a = a;
                _b = b;
            }
        }
    }
}