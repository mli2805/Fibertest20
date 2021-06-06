using System.Windows;
using System.Windows.Threading;

namespace Iit.Fibertest.Client
{
    public class UiDispatcherProvider : IDispatcherProvider
    {
        public Dispatcher GetDispatcher() { return Application.Current.Dispatcher; }
    }
}