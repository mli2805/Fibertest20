using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace WcfSelfhost
{
    [ServiceContract(CallbackContract = typeof(IBackward))]
    public interface IForward
    {
        [OperationContract]
        void BeginGetMessage(string message);

    }
    [ServiceContract]
    public interface IBackward
    {
        [OperationContract(IsOneWay = true)]
        void EndGetMessage(string result);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class Forward : IForward
    {
        public void BeginGetMessage(string message)
        {
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IBackward>();
          ThreadPool.QueueUserWorkItem(_ =>
          {
              try
              {
                  callbackChannel.EndGetMessage($"Hello {message}");
              }
              catch (Exception x)
              {
                  Console.WriteLine("Thread pool: " + x);
              }
          });
        }
    }

    public class Backward : IBackward
    {
        public Handler<string> ForMethodA { get; } = new Handler<string>();
        public Handler<Unit> ForMethodB { get; } = new Handler<Unit>();
        public void EndGetMessage(string result) => ForMethodA.End(result);
    }
    public sealed class Unit { }

    public class Handler<T>
    {
        private readonly Queue<TaskCompletionSource<T>> _handler = new Queue<TaskCompletionSource<T>>();
        public void AddHandler(TaskCompletionSource<T> handler) => _handler.Enqueue(handler);
        public void End(T result) => _handler.Dequeue().TrySetResult(result);
    }

    public static class ServiceExt
    {
        public static Task<string> GetMessageAsync(this IForward forward, Backward backward, string message)
        {
            var src = new TaskCompletionSource<string>();
            forward.BeginGetMessage(message);
            backward.ForMethodA.AddHandler(src);
            return src.Task;
        }
    }

    public class Class1
    {
        [Fact(Timeout = 3000000)]
        public async Task Test()
        {
            var host = new ServiceHost(new Forward(), new Uri("net.pipe://localhost"));
            // host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            // host.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
            try
            {
                host.AddServiceEndpoint(typeof(IForward),
                    new NetNamedPipeBinding(),
                    "net.pipe://localhost/PipeReverse");
                host.Open();

                var backward = new Backward();
                var channelFactory = new DuplexChannelFactory<IForward>(
                    backward,
                    new NetNamedPipeBinding(),
                    new EndpointAddress("net.pipe://localhost/PipeReverse"));
                var channel = channelFactory.CreateChannel();

                var result = await channel.GetMessageAsync(backward, "John");
                result.Should().Be("Hello John");
                channelFactory.Close();
            }
            catch (Exception x)
            {
                Console.WriteLine("Exception: ");
                Console.WriteLine(x);
            }
            finally
            {
                host.Close();
                Console.WriteLine("finally");
            }

        }
    }
}