using System;
using System.Threading.Tasks;

namespace Iit.Fibertest.Client
{
    public interface ILocalDbManager
    {
        Task SaveEvents(string[] events);
        Task<string[]> LoadEvents();
        void Initialize(Guid aggregateId, int snapshotLastEvent);

    }
}