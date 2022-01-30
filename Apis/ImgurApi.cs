using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace StatsStoreHelper.Apis
{
    public class ImgurApi
    {
        private static ImgurApi instance;
        private static readonly object instanceLock = new object();
        private HttpClient httpClient;
        private string clientId;

        private ImgurApi()
        {
            if(httpClient == null)
                httpClient = new HttpClient();

            // Hardcoded now; might need retrieving
            // It's Imgur's website clientId. Seems not changing for months
            // If uploading screenshots suddenly stops, it's first thing to check
            this.clientId = "546c25a59c58ad7";
        }

        public static ImgurApi GetInstance()
        {
            if(instance == null)
                lock(instanceLock)
                    if(instance == null)
                        instance = new ImgurApi();
            return instance;
        }

        public async Task<Dictionary<string, string>> UploadImage(byte[] image)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://api.imgur.com/3/image?client_id={clientId}");
            request.Content = new ByteArrayContent(image);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            // TODO: Handle connection errors

            JObject resultObject = JObject.Parse(result);
            string link = (string) resultObject["data"]["link"];
            string deleteHash = (string) resultObject["data"]["deletehash"];

            return new Dictionary<string, string>()
            {
                { "link", link },
                { "deletehash", deleteHash }
            };
        }

        public async Task DeleteImage(string deleteHash)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://imgur.com/delete/{deleteHash}");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "confirm", "true" }
            });
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // TODO: Handle connection errors
        }
    }
}