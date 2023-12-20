using Fibertest.RtuDaemon;
using Grpc.Net.Client;

namespace ClientNet6Grpc
{
    internal class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Hello, World!");

            using var channel = GrpcChannel.ForAddress("http://localhost:11942");
            var client = new Greeter.GreeterClient(channel);

            var response = await client.SayHelloAsync(new HelloRequest());
            Console.WriteLine($"Server response: {response.Message}");
            Console.ReadKey();

        }
    }
}