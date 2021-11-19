using BepInEx;
using BepInEx.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StatsStoreHelper
{
    public static class UserConfig
    {
        private static ConfigFile config = new ConfigFile(Path.Combine(Paths.ConfigPath, PluginInfo.PLUGIN_NAME, "config.cfg"), true);

        private static ConfigEntry<string> clientId;
        private static ConfigEntry<string> clientSecret;

        // TODO: Add more fields
        public static readonly Dictionary<string, string> StatsTags = new Dictionary<string, string>()
        {
            { "%date%", "Date" },
            { "%artist%", "Artist" },
            { "%album%", "Album" },
            { "%song%", "Song" },
            { "%genre%", "Genre" },
            { "%source%", "Source" },
            { "%charter%", "Charter" },
            { "%score%", "Score" },
            { "%multiplier%", "AVG Multiplier" },
            { "%stars%", "Stars" },
            { "%notes%", "Notes" },
            { "%accuracy%", "Accuracy" },
            { "%combo%", "Combo" },
            { "%sp%", "Star Powers" },
            { "%fc%", "FC" },
            { "%screenshot%", "Screenshot" },
            { "%screenshotdelete%", "Screenshot Delete Hash" },
            { "%hash%", "Hash" },
            { "%null%", "" }
        };

        private static ConfigEntry<string> statsRowFormat;
        private static ConfigEntry<string> statsPriority;

        public static void Load()
        {
            clientId = config.Bind<string>("Authorization", "ClientId", "", "Client Id received from Google Cloud Console");
            clientSecret = config.Bind<string>("Authorization", "ClientSecret", "", "Client Secret received from Google Cloud Console");
            statsRowFormat = config.Bind<string>(
                "Settings",
                "RowFormat",
                "%date% %artist% %song% %source% %charter% %score% %stars% %notes% %accuracy% %sp% %fc% %screenshot% %screenshotdelete% %hash%",
                "Order of columns in generated spreadsheet. Should not be changed after creating spreadsheet."
            );
            statsPriority = config.Bind<string>(
                "Settings",
                "StatsPriority",
                "%score% %fc% %accuracy% %notes% %stars%",
                "Which order should stats be compared in to decide which one is better."
            );
        }

        public static async Task Authorize()
        {
            ClientSecrets clientSecrets = new ClientSecrets
            {
                ClientId = clientId.Value,
                ClientSecret = clientSecret.Value
            };
            List<string> scopes = new List<string>
            {
                "https://www.googleapis.com/auth/photoslibrary.appendonly",
                "https://www.googleapis.com/auth/photoslibrary.readonly.appcreateddata",
                "https://www.googleapis.com/auth/drive.file",
            };
            
            // Note: Seems it throws an exception on failure when reading from file
            GoogleUserCredentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(Path.Combine(BepInEx.Paths.ConfigPath, PluginInfo.PLUGIN_NAME), true));
        }

        public static List<object> GetSheetHeaders()
        {
            string[] tags = statsRowFormat.Value.Split(' ');
            var headers = new List<object>();

            foreach(string tag in tags)
                if(StatsTags.ContainsKey(tag))
                    headers.Add(StatsTags[tag]);

            return headers;
        }

        // TODO: Load it from config file
        public static string DateTimeFormat => "yyyy-MM-dd HH:mm";

        public static List<string> UserStatsTags
        {
            get => new List<string>(statsRowFormat.Value.Split(' '));
        }

        public static UserCredential GoogleUserCredentials { get; private set; }
        public static List<string> UserStatsPriority
        {
            get => new List<string>(statsPriority.Value.Split(' '));
        }
    }
}