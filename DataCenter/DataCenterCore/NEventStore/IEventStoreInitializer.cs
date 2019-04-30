using NEventStore;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IEventStoreInitializer
    {
        string DataDir {get; }
        IStoreEvents Init();
        long GetDataSize();
        int OptimizeSorFilesTable();
        int RemoveCommitsUptoSnapshot(int lastEventNumber);

        void DropDatabase();
    }
}