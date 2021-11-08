using System;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;

namespace StatsStoreHelper
{
    public class StatsRow
    {
        public StatsRow()
        {
            this.RowData = new RowData();
            this.RowData.Values = new List<CellData>();
            StatsDict = new Dictionary<string, object>();
            foreach(string statTag in UserConfig.StatsTags.Keys)
                StatsDict.Add(statTag, null);
        }

        public StatsRow(RowData rowData) : this()
        {
            if(rowData != null)
                this.RowData = rowData;
            Load();
        }

        public void Load()
        {
            if(this.RowData.Values.Count < 1)
                return;

            foreach(string statTag in UserConfig.UserStatsTags)
            {
                int cellIndex = UserConfig.UserStatsTags.IndexOf(statTag);
                if(cellIndex >= this.RowData.Values.Count)
                    continue;

                ExtendedValue extendedValue = this.RowData.Values[cellIndex].UserEnteredValue;

                if(extendedValue.BoolValue != null)
                    StatsDict[statTag] = extendedValue.BoolValue;
                else if(extendedValue.NumberValue != null)
                    StatsDict[statTag] = extendedValue.NumberValue;
                else if(extendedValue.StringValue != null)
                    StatsDict[statTag] = extendedValue.StringValue;
            }
        }

        public int CompareTo(StatsRow otherStats)
        {
            foreach(string statTag in UserConfig.UserStatsPriority)
            {
                double thisStat = Convert.ToDouble(this.StatsDict[statTag]);
                double otherStat = Convert.ToDouble(otherStats.StatsDict[statTag]);
                if(thisStat < otherStat)
                    return 1;
                else if(thisStat > otherStat)
                    return -1;
            }
            return 0;
        }

        public Dictionary<string, object> StatsDict { get; private set; }
        public RowData RowData { get; private set; }
    }
}