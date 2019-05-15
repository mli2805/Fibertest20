using System;
using NEventStore;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IEventStoreInitializer
    {
        string DataDir {get; }
        Guid GetStreamIdIfExists();
        IStoreEvents Init();
        long GetDataSize();
        int OptimizeSorFilesTable();
        int RemoveCommitsUptoSnapshot(int lastEventNumber);

        void DropDatabase();
    }
}