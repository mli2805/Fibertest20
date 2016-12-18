using System;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

        public Task CreateNode(Coordinates coordinates)
        {
            return Post("api/graph/node", coordinates);
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

        private async Task<int> Post<T>(string url, T body)
        {
            var result = await _http.PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(body),
                    Encoding.UTF8, "application/json"));
            result.EnsureSuccessStatusCode();
            var response = await result.Content.ReadAsStringAsync();
            return Int32.Parse(response);
        }

        private async void Put<T>(string url, T body)
        {
            var result = await _http.PutAsync(url,
                new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
            result.EnsureSuccessStatusCode();
        }
        private async Task Delete(string url)
        {
            var result = await _http.DeleteAsync(url);
            result.EnsureSuccessStatusCode();
        }

        public void Dispose() => _http.Dispose();

    }
}