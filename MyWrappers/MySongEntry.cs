using System.IO;
using System.Security.Cryptography;
using HarmonyLib;

namespace StatsStoreHelper.MyWrappers
{
    public class MySongEntry
    {
        private SongEntry chSongEntry;
        public string iconName;
        public string folderPath;
        public bool isMIDIChart;

        public MySongEntry(object _chSongEntry)
        {
            this.chSongEntry = (SongEntry) _chSongEntry;
            this.iconName = this.chSongEntry.iconName;
            this.folderPath = this.chSongEntry.folderPath;
            this.isMIDIChart = this.chSongEntry.isMIDIChart;
        }

        public string Album
        {
            get => GetPropertyValue(this.chSongEntry.Album);
        }

        public string Artist
        {
            get => GetPropertyValue(this.chSongEntry.Artist);
        }

        public string Charter
        {
            // TODO: Strip html tags
            get => GetPropertyValue(this.chSongEntry.Charter);
        }

        public string Genre
        {
            get => GetPropertyValue(this.chSongEntry.Genre);
        }

        public string Name
        {
            get => GetPropertyValue(this.chSongEntry.Name);
        }

        public string Playlist
        {
            get => GetPropertyValue(this.chSongEntry.Playlist);
        }

        public string ChartPath
        {
            get => this.chSongEntry.chartPath;
        }

        private string GetPropertyValue(object property)
        {
            string propertyName = "\u030D\u031A\u030F\u0311\u0316\u030E\u031B\u0316\u0310\u0312\u0316";
            var propertyField = AccessTools.Field(property.GetType(), propertyName);
            return (string) propertyField.GetValue(property);
        }

        public string GetSHA256Hash()
        {
            using(SHA256Managed sha256 = new SHA256Managed())
            {
                using(FileStream file = File.OpenRead(this.ChartPath))
                {
                    string hash = "";
                    byte[] hashBytes = sha256.ComputeHash(file);
                    foreach(byte b in hashBytes)
                        hash += b.ToString("x2");
                    return hash;
                }
            }
        }

        // Note: Will come in handy for chorus db search
        public string GetMD5Hash()
        {
            using(MD5 md5 = MD5.Create())
            {
                using(FileStream file = File.OpenRead(this.ChartPath))
                {
                    string hash = "";
                    byte[] hashBytes = md5.ComputeHash(file);
                    foreach(byte b in hashBytes)
                        hash += b.ToString("x2");
                    return hash;
                }
            }
        }
    }
}