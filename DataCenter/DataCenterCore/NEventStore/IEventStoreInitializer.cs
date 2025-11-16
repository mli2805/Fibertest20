using System;
using NEventStore;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IEventStoreInitializer
    {
        string DataDir {get; }
        string ConnectionString { get; }
        Guid GetStreamIdIfExists();
        IStoreEvents Init();
        long GetDataSize();
        int OptimizeSorFilesTable();
        int RemoveCommitsIncludedIntoSnapshot(int lastEventNumber);

        void DropDatabase();
    }
}