using BepInEx;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace StatsStoreHelper
{
    public class GoogleSpreadsheet
    {
        private static GoogleSpreadsheet instance = null;
        private static readonly object instanceLock = new object(); 
        private UserCredential credentials;
        private SheetsService sheetsService;
        private string spreadsheetId;

        private GoogleSpreadsheet() {}

        public static GoogleSpreadsheet GetInstance()
        {
            if(instance == null)
                lock(instanceLock)
                    if(instance == null)
                        instance = new GoogleSpreadsheet();
            return instance;
        }
    
        public async void Init(UserCredential credentials, string name)
        {
            Name = name;
            this.credentials = credentials;
            
            GoogleApi.GetInstance().Init(credentials);
            
            sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = PluginInfo.PLUGIN_NAME
            });

            this.spreadsheetId = await GetSpreadsheetId();
            if(this.spreadsheetId == null)
                this.spreadsheetId = await CreateSpreadsheet();
        }

        private async Task<string> CreateSpreadsheet()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.Properties = new SpreadsheetProperties() { Title = Name };
            
            // TODO: Get player name from game
            spreadsheet.Sheets = new List<Sheet>() { CreateSheetTemplate("MGRINZ") };
            
            SpreadsheetsResource.CreateRequest createRequest = sheetsService.Spreadsheets.Create(spreadsheet);
            spreadsheet = await createRequest.ExecuteAsync();
            return spreadsheet.SpreadsheetId;
        }

        private async Task<string> GetSpreadsheetId()
        {
            string result = await GoogleApi.GetInstance().GetFileIdFromGoogleDrive(Name);
            return result;
        }

        private Sheet CreateSheetTemplate(string playerName)
        {            
            // TODO: Get this list from config
            List<object> headers = new List<object>
            {
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
            sheet.Data = GenerateGridData(new List<IList<object>>() { headers });

            CellFormat format = new CellFormat() {
                TextFormat = new TextFormat() { Bold = true }
            };
            sheet.Data = SetFormat(sheet.Data, 0, 0, headers.Count-1, sheet.Data.Count-1, format);
            return sheet;
        }

        private IList<GridData> GenerateGridData(IList<IList<object>> plainData)
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

        private IList<GridData> SetFormat(IList<GridData> gridDatas, int colStart, int rowStart, int colEnd, int rowEnd, CellFormat format)
        {
            foreach(GridData gridData in gridDatas)
            {
                if(gridData.StartColumn > colEnd || gridData.StartRow > rowEnd)
                    continue;

                IList<RowData> rowDatas = gridData.RowData;
                int rowI = gridData.StartRow.GetValueOrDefault();
                foreach(RowData rowData in rowDatas)
                {
                    if(rowI < rowStart)
                    {
                        rowI++;
                        continue;
                    }
                    
                    if(rowI > rowEnd)
                        break;

                    IList<CellData> cellDatas = rowData.Values;
                    int colI = gridData.StartColumn.GetValueOrDefault();
                    foreach(CellData cellData in cellDatas)
                    {
                        if(colI < colStart)
                        {
                            colI++;
                            continue;
                        }
                        if(colI > colEnd)
                            break;
                            
                        cellData.UserEnteredFormat = format;

                        colI++;
                    }

                    rowI++;
                }
            }
            return gridDatas;
        }

        public string Name { get; private set; }

    }
}