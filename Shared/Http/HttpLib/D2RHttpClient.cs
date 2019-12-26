using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpLib
{
    public class D2RHttpClient
    {
        private static readonly HttpClient client = new HttpClient();
        public async Task<string> GetAsync(string uri)
        {
            var response = await client.GetAsync(uri);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
            //  return await client.GetStringAsync(uri);
        }

      

        public async Task<string> PostAsync(string uri)
        {
            var values = new Dictionary<string, string>
            {
                { "thing1", "hello" },
                { "thing2", "world" }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(uri, content);

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
    }
}