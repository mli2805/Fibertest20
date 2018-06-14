using NEventStore;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IEventStoreInitializer
    {
        IStoreEvents Init();
    }
}