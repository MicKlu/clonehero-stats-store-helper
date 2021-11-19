using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StatsStoreHelper.Utils
{
    public class StatsRowBuilder
    {
        // TODO: Add/verify all known "official" icons
        private static readonly Dictionary<string, string> IconSources = new Dictionary<string, string> ()
        {
            { "gh", "Guitar Hero" },
            { "gh2", "Guitar Hero II" },
            { "gh2dlc", "Guitar Hero II DLC" },
            { "gh80s", "Guitar Hero Encore: Rocks the 80s" },
            { "gh3", "Guitar Hero III: Legends of Rock" },
            { "gh3dlc", "Guitar Hero III: Legends of Rock DLC" },
            { "gha", "Guitar Hero: Aerosmith" },
            { "ghwt", "Guitar Hero World Tour" },
            { "ghwtdlc", "Guitar Hero World Tour DLC" },
            { "ghm", "Guitar Hero: Metallica" },
            { "ghmdlc", "Guitar Hero: Metallica DLC" },
            { "ghsh", "Guitar Hero Smash Hits" },
            { "ghv", "Guitar Hero: Van Halen" },
            { "gh5", "Guitar Hero 5" },
            { "gh5dlc", "Guitar Hero 5 DLC" },
            { "bh", "Band Hero" },
            { "ghwor", "Guitar Hero: Warriors of Rock" },
            { "ghwordlc", "Guitar Hero: Warriors of Rock DLC" },
            { "ghl", "Guitar Hero Live" },
            { "rb1", "Rock Band" },
            { "rb1dlc", "Rock Band DLC" },
            { "rb2", "Rock Band 2" },
            { "rb2dlc", "Rock Band 2 DLC" },
            { "rb3", "Rock Band 3" },
            { "rb3dlc", "Rock Band 3 DLC" },
            { "rb4", "Rock Band 4" },
            { "rb4dlc", "Rock Band 4 DLC" },
            // { "", "The Beatles: Rock Band" },
            // { "", "Green Day: Rock Band" },
            { "lrb", "Lego Rock Band" },
            { "rbb", "Rock Band Blitz" },
            { "rbn", "Rock Band Network" },
            { "ccc", "Customs Creators Collective" }
        };
        private StatsRow row;
        public StatsRowBuilder() {
            Reset();
        }

        public void Reset()
        {
            row = new StatsRow();
        }

        public StatsRowBuilder AddStat(string statTag, object value)
        {
            CellData cell = GetFormatedCell(statTag, value);
            row.RowData.Values.Add(cell);
            return this;
        }

        public StatsRowBuilder ReplaceStat(string statTag, object value)
        {
            int statIndex = UserConfig.UserStatsTags.IndexOf(statTag);
            CellData cell = GetFormatedCell(statTag, value);
            row.RowData.Values[statIndex] = cell;
            return this;
        }

        public static CellData GetFormatedCell(string statTag, object value)
        {
            CellData cell = new CellData();
            cell.UserEnteredValue = new ExtendedValue();

            // Value switch
            switch(statTag)
            {
                // TODO: Add more fields
                case "%date%":
                {
                    DateTime date = (DateTime) value;
                    double serialFormatDate = 0;

                    TimeSpan timeSpan = date.Subtract(new DateTime(1899, 12, 30));
                    serialFormatDate += timeSpan.TotalDays;
                    serialFormatDate += ((date.Hour * 60 + date.Minute) * 60 + date.Second)  / (24 * 60 * 60);

                    cell.UserEnteredValue.NumberValue = serialFormatDate;
                    break;
                }
                case "%source%":
                {
                    var source = value.ToString();
                    if(IconSources.ContainsKey(source))
                        source = IconSources[source];
                    else
                        source = "Unknown Source " + source;

                    // TODO: check chorus db
                    // TODO: check if is from Custom Songs Central

                    cell.UserEnteredValue.StringValue = source;
                    break;
                }
                case "%charter%":
                {
                    var charter = value.ToString();
                    charter = Regex.Replace(charter, @"<(.*?)(.*?)?>(.*?)</\1>", @"$3");
                    charter = Regex.Replace(charter, " +", " ");
                    cell.UserEnteredValue.StringValue = charter;
                    break;
                }
                case "%score%":
                case "%stars%":
                case "%accuracy%":
                case "%combo%":
                case "%multiplier%":
                {
                    cell.UserEnteredValue.NumberValue = Convert.ToDouble(value);
                    break;
                }
                case "%fc%":
                {
                    cell.UserEnteredValue.BoolValue = (bool) value;
                    break;
                }
                default:
                {
                    cell.UserEnteredValue.StringValue = value.ToString();
                    break;
                }
            }

            // Format switch
            CellFormat cellFormat = new CellFormat();
            switch(statTag)
            {
                // TODO: Add more fields
                case "%date%":
                {
                    cellFormat.NumberFormat = new NumberFormat() { Type = "DATE_TIME" };
                    break;
                }
                case "%score%":
                {
                    cellFormat.NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "#,##0" };
                    break;
                }
                case "%accuracy%":
                {
                    cellFormat.NumberFormat = new NumberFormat() { Type = "PERCENT", Pattern = "0.00%" };
                    break;
                }
                case "%screenshot%":
                {
                    TextFormat textFormat = new TextFormat();
                    textFormat.Link = new Link() { Uri = value.ToString() };

                    TextFormatRun textFormatRun = new TextFormatRun();
                    textFormatRun.StartIndex = 0;
                    textFormatRun.Format = textFormat;
                    
                    cell.TextFormatRuns = new List<TextFormatRun>() { textFormatRun };
                    break;
                }
            }
            cell.UserEnteredFormat = cellFormat;
            return cell;
        }

        public StatsRow Build()
        {
            row.Load();
            return row;
        } 
    }
}