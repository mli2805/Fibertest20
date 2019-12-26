using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace D2RtuVeexManager
{
    public static class HttpClientExt
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content) 
        {
            var method = new HttpMethod("PATCH");

            var request = new HttpRequestMessage(method, requestUri) { Content = content };

            // In case you want to set a timeout
            //CancellationToken cancellationToken = new CancellationTokenSource(60).Token;

            try 
            {
                return await client.SendAsync(request);
                // If you want to use the timeout you set
                //response = await client.SendAsync(request).AsTask(cancellationToken);
            } 
            catch(Exception e) 
            {
                Console.WriteLine("PatchAsync: "+ e.Message);
                return null;
            }
        }
    }
}