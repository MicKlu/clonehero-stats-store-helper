using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StatsStoreHelper
{
    public static class UserConfig
    {
        // TODO: Add more fields
        private static readonly Dictionary<string, string> HeaderNames = new Dictionary<string, string>()
        {
            { "%date%", "Date" },
            { "%artist%", "Artist" },
            { "%song%", "Song" },
            { "%source%", "Source" },
            { "%charter%", "Charter" },
            { "%score%", "Score" },
            { "%stars%", "Stars" },
            { "%accuracy%", "Accuracy" },
            { "%sp%", "Star Powers" },
            { "%fc%", "FC" },
            { "%screenshot%", "Screenshot" },
            { "%hash%", "Hash" }
        };

        // TODO: Load it from config file
        private static string statsRowFormat = 
            "%date% %artist% %song% %source% %charter% %score% %stars% %accuracy% %sp% %fc% %screenshot% %hash%";

        public static async Task Authorize()
        {
            ClientSecrets clientSecrets = new ClientSecrets
            {
                ClientId = "1043533161342-rb1s1n4pcstiuc58tptkj3a6ju611guo.apps.googleusercontent.com",
                ClientSecret = "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"
            };
            List<string> scopes = new List<string>
            {
                "https://www.googleapis.com/auth/photoslibrary.appendonly",
                "https://www.googleapis.com/auth/photoslibrary.readonly.appcreateddata",
                "https://www.googleapis.com/auth/drive.file"
            };
            
            GoogleUserCredentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore($"{BepInEx.Paths.ConfigPath}/{PluginInfo.PLUGIN_NAME}", true));
        }

        public static List<object> GetSheetHeaders()
        {
            string[] tags = statsRowFormat.Split(' ');
            var headers = new List<object>();

            foreach(string tag in tags)
                if(HeaderNames.ContainsKey(tag))
                    headers.Add(HeaderNames[tag]);

            return headers;
        }

        // TODO: Load it from config file
        public static string DateTimeFormat => "yyyy-MM-dd HH:mm";

        public static List<string> StatsTags
        {
            get
            {
                return new List<string>(statsRowFormat.Split(' '));
            }
        }

        public static UserCredential GoogleUserCredentials { get; private set; }
    }
}