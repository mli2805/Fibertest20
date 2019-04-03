using System;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.UtilsLib;
using NEventStore;

namespace Graph.Tests
{
    public sealed class FakeEventStoreInitializer : IEventStoreInitializer
    {
        private readonly IMyLog _logFile;

        public FakeEventStoreInitializer(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public string DataDir => @"memory";

        public IStoreEvents Init()
        {
            try
            {
                var storeEvents = Wireup.Init()
                    .UsingInMemoryPersistence()
                    .InitializeStorageEngine()
                    .Build();

                _logFile.AppendLine(@"EventStoreService initialized successfully");
                return storeEvents;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public long GetDataSize()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }
    }
}