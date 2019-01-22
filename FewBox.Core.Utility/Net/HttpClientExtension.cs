using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace FewBox.Core.Utility.Net
{
    public static class HttpClientExtension
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent iContent)
    {
        var method = new HttpMethod("PATCH");
        var request = new HttpRequestMessage(method, requestUri)
        {
            Content = iContent
        };

        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            response = await client.SendAsync(request);
        }
        catch (TaskCanceledException e)
        {
            Debug.WriteLine("ERROR: " + e.ToString());
        }

        return response;
    }
    }
}