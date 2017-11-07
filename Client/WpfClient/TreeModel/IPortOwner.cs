using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public interface IPortOwner
    {
        ChildrenImpresario ChildrenImpresario { get; }
        NetAddress OtauNetAddress { get; set; }
    }
}