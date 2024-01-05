using System.Net.Http.Json;
using System.Text;
using Fibertest.RtuDaemon;
using Grpc.Net.Client;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.ClientNet6Grpc
{
    internal class Program
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        static async Task Main()
        {
            Console.WriteLine(@"Hello, World!");

            using var channel = GrpcChannel.ForAddress("http://localhost:11942");
            var client = new Greeter.GreeterClient(channel);

            var response = await client.SayHelloAsync(new HelloRequest());
            Console.WriteLine($@"Server gRPC response: {response.Message}");


            var httpClient = new HttpClient();
            var dto = new InitializeRtuDto();
            var json = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var httpResponse =
                await httpClient.PostAsync("http://localhost:11980/rtu/enqueue-long-operation", stringContent);
            var answer = await httpResponse.Content.ReadFromJsonAsync<RequestAnswer>();
            Console.WriteLine(answer != null
                ? $"Server HTTP answer: {answer.ReturnCode}"
                : "Failed to parse HTTP response!");

            var dto2 = new ApplyMonitoringSettingsDto();
            using var httpResponse2 =
                await httpClient.PostAsJsonAsyncMy("http://localhost:11980/rtu/enqueue-long-operation", dto2);
            var answer2 = await httpResponse2.Content.ReadFromJsonAsync<RequestAnswer>();
            Console.WriteLine(answer2 != null
                ? $"Server HTTP answer: {answer2.ReturnCode}"
                : "Failed to parse HTTP response!");

            using var httpResponse3 =
                await httpClient.GetAsync("http://localhost:11980/rtu/current-state");
            var answer3 = await httpResponse3.Content.ReadFromJsonAsync<RequestAnswer>();
            Console.WriteLine(answer3 != null
                ? $"Server HTTP answer: {answer3.ReturnCode}"
                : "Failed to parse HTTP response!");

            using var httpResponse4 =
                          await httpClient.GetAsync("http://localhost:11980/rtu/messages");
            var answer4 = await httpResponse4.Content.ReadFromJsonAsync<List<string>>();
            Console.WriteLine(answer4 != null
                ? $"Server HTTP answer: {answer4.Count}"
                : "null");


            Console.ReadKey();

        }
    }

    public static class HttpClientExt
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public static async Task<HttpResponseMessage> PostAsJsonAsyncMy<T>(this HttpClient client, string requestUrl, T theObj)
        {
            var stringContent = new StringContent(
                JsonConvert.SerializeObject(theObj, JsonSerializerSettings),
                Encoding.UTF8, "application/json");
            return await client.PostAsync(requestUrl, stringContent);
        }
    }
}