using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace StatsStoreHelper.Apis.GoogleApi
{
    public class GoogleApi
    {
        private static GoogleApi instance = null;
        private static readonly object instanceLock = new object();
        private UserCredential credentials;
        private string googleAlbumTitle;

        public string GooglePhotosAlbumId { get; private set; }

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

        public async void Init(UserCredential credentials, string googleAlbumTitle)
        {
            if(httpClient == null)
                httpClient = new HttpClient();
            
            this.credentials = credentials;
            this.googleAlbumTitle = googleAlbumTitle;
            
            GooglePhotosAlbumId = await GetAlbumIdFromGooglePhotos();
            if(GooglePhotosAlbumId == null)
                GooglePhotosAlbumId = await CreatePhotosAlbum();
        }

        private async Task<HttpRequestMessage> CreateRequest(HttpMethod method, string requestUri)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
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

        public async Task<string> UploadToGooglePhotos(byte[] file)
        {
            HttpRequestMessage request = await CreateRequest(HttpMethod.Post, "https://photoslibrary.googleapis.com/v1/uploads");
            request.Headers.Add("X-Goog-Upload-Content-Type", "image/png");
            request.Headers.Add("X-Goog-Upload-Protocol", "raw");
            request.Content = new ByteArrayContent(file);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // TODO: Handle connection errors

            string token = await response.Content.ReadAsStringAsync();
            return token;
        }

        public async Task<string> CreateMediaItemInGooglePhotos(object fileName, object uploadToken)
        {
            HttpRequestMessage request = await CreateRequest(HttpMethod.Post, "https://photoslibrary.googleapis.com/v1/mediaItems:batchCreate");
            
            var simpleMediaItem = new Dictionary<string, object>();
            simpleMediaItem.Add("fileName", fileName);
            simpleMediaItem.Add("uploadToken", uploadToken);

            var newMediaItems = new Dictionary<string, object>();
            newMediaItems.Add("simpleMediaItem", simpleMediaItem);

            var body = new Dictionary<string, object>();
            body.Add("albumId", GooglePhotosAlbumId);
            
            body.Add("newMediaItems", newMediaItems);

            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            
            string result = await response.Content.ReadAsStringAsync();
            JObject resultObject = JObject.Parse(result);
            System.Console.WriteLine(result);
            return (string) resultObject["newMediaItemResults"].Children().ToList()[0]["mediaItem"]["productUrl"];
        }

        public async Task<string> CreatePhotosAlbum()
        {
            HttpRequestMessage request = await CreateRequest(HttpMethod.Post, "https://photoslibrary.googleapis.com/v1/albums");
            Dictionary<string, object> body = new Dictionary<string, object>()
            {
                { "album", new Dictionary<string, string>() { { "title", this.googleAlbumTitle } } }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await httpClient.SendAsync(request);

            string result = await response.Content.ReadAsStringAsync();
            JObject resultObject = JObject.Parse(result);
            GooglePhotosAlbumId = (string) resultObject["id"];

            request = await CreateRequest(HttpMethod.Post, $"https://photoslibrary.googleapis.com/v1/albums/{GooglePhotosAlbumId}:share");
            response = await httpClient.SendAsync(request);

            return GooglePhotosAlbumId;
        }

        private async Task<string> GetAlbumIdFromGooglePhotos()
        {
            HttpRequestMessage request = await CreateRequest(HttpMethod.Get, "https://photoslibrary.googleapis.com/v1/albums");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            // TODO: Handle connection errors

            string result = await response.Content.ReadAsStringAsync();
            JObject resultObject = JObject.Parse(result);

            if(!resultObject.ContainsKey("albums"))
                return null;

            List<JToken> albums = resultObject["albums"].Children().ToList();

            foreach(var album in albums)
                if((string) album["title"] == this.googleAlbumTitle)
                    return (string) album["id"];

            return null;
        }

    }
}