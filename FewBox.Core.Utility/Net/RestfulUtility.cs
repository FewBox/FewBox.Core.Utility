using FewBox.Core.Utility.Formatter;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FewBox.Core.Utility.Net
{
    public static class RestfulUtility
    {
        public static O Post<B, O>(string url, Package<B> package) where O : class
        {
            string responseString = String.Empty;
            return WapperHttpClient<O>((httpClient) => {
                return httpClient.PostAsync(url, ConvertBodyObjectToStringContent(package.Body));
            }, package.Headers);
        }

        public static O Put<B, O>(string url, Package<B> package) where O : class
        {
            return WapperHttpClient<O>((httpClient) =>
            {
                return httpClient.PutAsync(url, ConvertBodyObjectToStringContent(package.Body));
            }, package.Headers);
        }

        public static O Patch<B, O>(string url, Package<B> package) where O : class
        {
            return WapperHttpClient<O>((httpClient) =>
            {
                return httpClient.PatchAsync(url, ConvertBodyObjectToStringContent(package.Body));
            }, package.Headers);
        }
        
        public static O Delete<O>(string url, IList<Header> headers) where O : class
        {
            return WapperHttpClient<O>((httpClient) =>
            {
                return httpClient.DeleteAsync(url);
            }, headers);
        }

        public static O Get<O>(string url, IList<Header> headers) where O : class
        {
            return WapperHttpClient<O>((httpClient) =>
            {
                return httpClient.GetAsync(url);
            }, headers);
        }

        private static StringContent ConvertBodyObjectToStringContent<T>(T body)
        {
            string jsonString = JsonUtility.Serialize<T>(body);
            return new StringContent(jsonString, Encoding.UTF8, "application/json");
        }

        private static void InitHeadersObjectToHttpRequestHeaders(HttpClient httpClient, IList<Header> headers)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", "FewBox Validation");
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        private static O WapperHttpClient<O>(Func<HttpClient, Task<HttpResponseMessage>> action, IList<Header> headers) where O : class
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(1);
                InitHeadersObjectToHttpRequestHeaders(httpClient, headers);
                return GetResponse<O>(action(httpClient));
            }
        }

        private static O GetResponse<O>(Task<HttpResponseMessage> task) where O : class
        {
            O response = default(O);
            task.ContinueWith((requestTask) =>
                {
                    HttpResponseMessage httpResponseMessage = requestTask.Result;
                    httpResponseMessage.EnsureSuccessStatusCode();
                    return httpResponseMessage.Content.ReadAsStringAsync().Result;
                }
            )
            .ContinueWith((readTask) =>
                {
                    response = JsonUtility.Deserialize<O>(readTask.Result);
                }
            )
            .Wait();
            return response;
        }
    }
}