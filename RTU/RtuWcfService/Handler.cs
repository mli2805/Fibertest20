using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iit.Fibertest.RtuWcfServiceInterface
{
    public class Handler<T>
    {
        private readonly Queue<TaskCompletionSource<T>> _handler = new Queue<TaskCompletionSource<T>>();
        public void AddHandler(TaskCompletionSource<T> handler) => _handler.Enqueue(handler);
        public void End(T result) => _handler.Dequeue().TrySetResult(result);
    }
}