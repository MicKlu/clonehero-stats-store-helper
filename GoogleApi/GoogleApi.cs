using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace StatsStoreHelper.GoogleApi
{
    public class GoogleApi
    {
        private static GoogleApi instance = null;
        private static readonly object instanceLock = new object();
        private UserCredential credentials;
        private static HttpClient httpClient = null;

        private GoogleApi() {}

        public static GoogleApi GetInstance()
        {
            if(instance == null)
                lock(instanceLock)
                    if(instance == null)
                        instance = new GoogleApi();
            return instance;
        }

        public void Init(UserCredential credentials)
        {
            if(httpClient == null)
                httpClient = new HttpClient();
            
            this.credentials = credentials;
        }

        private async Task<HttpRequestMessage> CreateRequest(HttpMethod method, string requestUri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            await credentials.RefreshTokenAsync(CancellationToken.None);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.Token.AccessToken);
            return request;
        }

        public async Task<string> GetFileIdFromGoogleDrive(string fileName)
        {
            HttpRequestMessage request = await CreateRequest(HttpMethod.Get, "https://content.googleapis.com/drive/v3/files");
            request.Properties.Add("q", $"name = \"{fileName}\"");
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // TODO: Handle connection errors

            string result = await response.Content.ReadAsStringAsync();
            JObject resultObject = JObject.Parse(result);
            List<JToken> files = resultObject["files"].Children().ToList();

            if(files.Count > 0)
                if((string) files[0]["mimeType"] == "application/vnd.google-apps.spreadsheet")
                    return (string) files[0]["id"];

            return null;
        }

        public async Task<string> GetRequest(string requestUri, Dictionary<string, string> query)
        {
            string queryString = "?";
            HttpRequestMessage request = await CreateRequest(HttpMethod.Get, requestUri);

            foreach(KeyValuePair<string, string> queryPair in query)
                queryString += $"{queryPair.Key}={WebUtility.UrlEncode(queryPair.Value)}&";

            HttpResponseMessage response = await httpClient.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();
            return result;
        }

    }
}