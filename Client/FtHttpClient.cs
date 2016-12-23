using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Client.Domain;
using CommonLogic.Database;
using Newtonsoft.Json;

namespace Client
{
    public sealed class FtHttpClient : IDisposable
    {
        private readonly HttpClient _http;

        public FtHttpClient(HttpClient http)
        {
            _http = http;
        }

        public ClientNode CreateNode(ClientNode clientNode)
        {
            DbNode dbNode = new DbNode();

            // mapping

            dbNode = Post("api/graph/node", dbNode).Result;

            // mapping 

            return clientNode;
        }

        public void UpdateNode(ClientNode clientNode)
        {
            var url = $"api/graph/node/{clientNode.Id}";
            Put(url, clientNode);
        }
        public Task RemoveNode(int id)
        {
            return Delete("api/graph/node/id");
        }

        public Task<Graph> GetGraph()
        {
            return Get<Graph>("api/graph");
        }
        private async Task<T> Get<T>(string uri)
        {
            var result = await _http.GetAsync(uri);
            result.EnsureSuccessStatusCode();
            var response = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(response);
        }

        private async Task<T> Post<T>(string url, T body)
        {
            var result = await _http.PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(body),
                    Encoding.UTF8, "application/json"));
            result.EnsureSuccessStatusCode();
            var response = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(response);
        }

        private async void Put<T>(string url, T body)
        {
            var response = await _http.PutAsync(url,
                new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
        }
        private async Task Delete(string url)
        {
            var result = await _http.DeleteAsync(url);
            result.EnsureSuccessStatusCode();
        }

        public void Dispose() => _http.Dispose();

    }
}