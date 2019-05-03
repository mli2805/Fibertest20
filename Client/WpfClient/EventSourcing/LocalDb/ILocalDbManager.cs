using System.Threading.Tasks;

namespace Iit.Fibertest.Client
{
    public interface ILocalDbManager
    {
        Task SaveEvents(string[] events);
        Task<string[]> LoadEvents();

        Task<int> SaveSnapshot(byte[] portion);
        Task<byte[]> LoadSnapshot();

        void Initialize();

    }
}