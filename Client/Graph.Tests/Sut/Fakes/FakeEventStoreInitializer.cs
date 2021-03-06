﻿using System;
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

        public Guid GetStreamIdIfExists()
        {
            return Guid.Empty;
        }

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

        public int OptimizeSorFilesTable()
        {
            throw new NotImplementedException();
        }

        public int RemoveCommitsIncludedIntoSnapshot(int lastEventNumber)
        {
            return 0;
        }

        public void DropDatabase()
        {
            throw new NotImplementedException();
        }
    }
}