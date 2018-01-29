using System;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.UtilsLib;
using NEventStore;

namespace Graph.Tests
{
    public sealed class FakeEventStoreInitializer : IEventStoreInitializer
    {
        public IStoreEvents Init(IMyLog logFile)
        {
            try
            {
                var storeEvents = Wireup.Init()
                    .UsingInMemoryPersistence()
                    .InitializeStorageEngine()
                    .Build();

                logFile.AppendLine(@"EventStoreService initialized successfully");
                return storeEvents;
            }
            catch (Exception e)
            {
                logFile.AppendLine(e.Message);
                return null;
            }
        }
    }
}