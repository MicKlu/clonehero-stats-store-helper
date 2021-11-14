using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;

namespace StatsStoreHelper.Utils
{
    public class StatsRowBuilder
    {
        private StatsRow row;
        public StatsRowBuilder() {
            Reset();
        }

        private void Reset()
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

        private CellData GetFormatedCell(string statTag, object value)
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
                case "%score%":
                case "%stars%":
                case "%accuracy%":
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