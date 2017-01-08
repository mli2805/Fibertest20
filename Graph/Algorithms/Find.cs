using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iit.Fibertest.Graph
{
    public static class Find
    {
        public static Guid FindFiberByNodes(this ReadModel readModel, Guid leftNode, Guid rightNode)
        {
            foreach (var fiber in readModel.Fibers)
            {
                if ((fiber.Node1 == leftNode || fiber.Node1 == rightNode) &&
                    (fiber.Node2 == leftNode || fiber.Node2 == rightNode))
                    return fiber.Id;

            }
            return Guid.Empty;
        }
    }
}
