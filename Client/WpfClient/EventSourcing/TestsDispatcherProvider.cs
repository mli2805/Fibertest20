using System.Windows.Threading;

namespace Iit.Fibertest.Client
{
    public class TestsDispatcherProvider : IDispatcherProvider
    {
        public Dispatcher GetDispatcher() { return Dispatcher.CurrentDispatcher; }
    }
}