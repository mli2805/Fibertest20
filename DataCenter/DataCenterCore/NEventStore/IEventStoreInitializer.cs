using NEventStore;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IEventStoreInitializer
    {
        string DataDir {get; }
        IStoreEvents Init();
        long GetDataSize();
        void Delete();
    }
}