using HarmonyLib;

namespace StatsStoreHelper.MyWrappers
{
    public class MySongEntry
    {
        private SongEntry chSongEntry;
        public string iconName;

        public MySongEntry(object _chSongEntry)
        {
            this.chSongEntry = (SongEntry) _chSongEntry;
            this.iconName = this.chSongEntry.iconName;
        }

        public string Album
        {
            get { return GetPropertyValue(this.chSongEntry.Album); }
        }

        public string Artist
        {
            get { return GetPropertyValue(this.chSongEntry.Artist); }
        }

        public string Charter
        {
            get { return GetPropertyValue(this.chSongEntry.Charter); }
        }

        public string Genre
        {
            get { return GetPropertyValue(this.chSongEntry.Genre); }
        }

        public string Name
        {
            get { return GetPropertyValue(this.chSongEntry.Name); }
        }

        public string Playlist
        {
            get { return GetPropertyValue(this.chSongEntry.Playlist); }
        }

        private string GetPropertyValue(object property)
        {
            string propertyName = "\u030D\u031A\u030F\u0311\u0316\u030E\u031B\u0316\u0310\u0312\u0316";
            var propertyField = AccessTools.Field(property.GetType(), propertyName);
            return (string) propertyField.GetValue(property);
        }
    }
}