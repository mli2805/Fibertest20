using System.Threading.Tasks;

namespace Iit.Fibertest.Client
{
    public interface ILocalDbManager
    {
        Task SaveEvents(string[] events, int eventId);
        Task<CacheParameters> GetCacheParameters();
        Task<string[]> LoadEvents(int lastEventInSnapshot);

        Task<int> SaveSnapshot(byte[] portion);
        Task<byte[]> LoadSnapshot(int lastEventInSnapshotOnServer);

        Task<bool> RecreateCacheDb();
        void Initialize();

    }
}