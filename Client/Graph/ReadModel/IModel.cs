using System.Collections.Generic;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public interface IModel
    {
        IMyLog LogFile { get; }

        List<Node> Nodes { get; }
        List<Fiber> Fibers { get; }
        List<Equipment> Equipments { get; }
        List<Rtu> Rtus { get; }
        List<Trace> Traces { get; }
        List<Otau> Otaus { get; }
        List<User> Users { get; }
        List<Zone> Zones { get; }

        string RemoveNodeWithAllHis(System.Guid nodeId);
    }
}