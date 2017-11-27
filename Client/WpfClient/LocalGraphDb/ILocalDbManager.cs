namespace Iit.Fibertest.Client
{
    public interface ILocalDbManager
    {
        void SaveEvents(string[] events);
        string[] LoadEvents();
    }
}