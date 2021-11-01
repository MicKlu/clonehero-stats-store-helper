using BepInEx;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace StatsStoreHelper
{
    public class GoogleSpreadsheet
    {
        private static GoogleSpreadsheet instance = null;
        private static readonly object instanceLock = new object(); 
        private UserCredential credentials;
        private SheetsService sheetsService;

        private GoogleSpreadsheet()
        {
            
        }

        public static GoogleSpreadsheet GetInstance()
        {
            if(instance == null)
                lock(instanceLock)
                    if(instance == null)
                        instance = new GoogleSpreadsheet();
            return instance;
        }
    
        public void init(UserCredential credentials, string name)
        {
            Name = name;
            this.credentials = credentials;
            
            sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = PluginInfo.PLUGIN_NAME
            });
            CreateSpreadsheet();
        }

        private async void CreateSpreadsheet()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.Properties = new SpreadsheetProperties() { Title = Name };
            
            // TODO: Get player name from game
            spreadsheet.Sheets = new List<Sheet>() { CreateSheetTemplate("MGRINZ") };
            
            SpreadsheetsResource.CreateRequest createRequest = sheetsService.Spreadsheets.Create(spreadsheet);
            spreadsheet = await createRequest.ExecuteAsync();
            System.Console.WriteLine(JsonConvert.SerializeObject(spreadsheet));
        }

        private Sheet CreateSheetTemplate(string playerName)
        {            
            // TODO: Get this list from config
            List<object> headers = new List<object> {
                "",
                "Setlist order",
                "Artist",
                "Song",
                "Source",
                "Charter",
                "Score",
                "Stars",
                "Accuracy",
                "Star Powers",
                "FC",
                "Screenshot"
            };

            Sheet sheet = new Sheet();
            SheetProperties properties = new SheetProperties() { Title = playerName };
            sheet.Properties = properties;
            sheet.Data = GenerateGridData(new List<List<object>>() { headers });
            return sheet;
        }

        private List<GridData> GenerateGridData(List<List<object>> plainData)
        {
            List<RowData> rows = new List<RowData>();
            foreach(List<object> plainRow in plainData)
            {   
                List<CellData> cols = new List<CellData>();
                foreach(object plainCol in plainRow)
                {
                    CellData cell = new CellData() { UserEnteredValue = new ExtendedValue() { StringValue = plainCol.ToString() }};
                    cols.Add(cell);
                }
                RowData row = new RowData() { Values = cols };
                rows.Add(row);
            }

            List<GridData> grid = new List<GridData>();
            grid.Add(new GridData() { RowData = rows });

            return grid;
        }

        public string Name { get; private set; }

    }
}