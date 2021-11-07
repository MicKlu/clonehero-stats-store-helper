using BepInEx;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System;
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
        private int sheetId = 0;
        private string sheetName;

        private GoogleSpreadsheet() {}

        public static GoogleSpreadsheet GetInstance()
        {
            if(instance == null)
                lock(instanceLock)
                    if(instance == null)
                        instance = new GoogleSpreadsheet();
            return instance;
        }
    
        public async Task Init(UserCredential credentials, string spreadsheetName, string sheetName)
        {
            if(this.credentials != credentials)
            {
                this.credentials = credentials;

                System.Console.WriteLine("Initializing GoogleApi");
                GoogleApi.GetInstance().Init(credentials);

                System.Console.WriteLine("Initializing SheetsService");
                sheetsService = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = PluginInfo.PLUGIN_NAME
                });

                this.spreadsheetId = null;
                this.sheetId = 0;
            }

            if(this.Name != spreadsheetName)
            {
                this.spreadsheetId = null;
                this.sheetId = 0;
            }

            if(this.spreadsheetId == null)
            {
                this.Name = spreadsheetName;
                this.SheetName = sheetName;

                this.spreadsheetId = await GetSpreadsheetId();
                if(this.spreadsheetId == null)
                {
                    Spreadsheet spreadsheet = await CreateSpreadsheet();
                    this.spreadsheetId = spreadsheet.SpreadsheetId;
                    this.sheetId = (int) spreadsheet.Sheets[0].Properties.SheetId;
                }
            }

            if(this.SheetName != sheetName)
                this.sheetId = 0;
            
            if(this.sheetId == 0)
            {
                this.SheetName = sheetName;
                this.sheetId = await GetSheetId(sheetName);
                if(this.spreadsheetId == null)
                    this.sheetId = await AddSheet(CreateSheetTemplate());
            }
        }

        private Task<int> AddSheet(Sheet sheet)
        {
            throw new NotImplementedException();
        }

        public async void AppendRow(RowData row)
        {
            Request request = new Request();
            request.AppendCells = new AppendCellsRequest();
            request.AppendCells.SheetId = await GetSheetId(SheetName);
            request.AppendCells.Rows = new List<RowData>() { row };
            request.AppendCells.Fields = "*";

            BatchUpdateSpreadsheetRequest body = new BatchUpdateSpreadsheetRequest();
            body.Requests = new List<Request>() { request };
            SpreadsheetsResource.BatchUpdateRequest batchUpdateRequest = sheetsService.Spreadsheets.BatchUpdate(body, spreadsheetId);

            BatchUpdateSpreadsheetResponse response = await batchUpdateRequest.ExecuteAsync();
        }

        public async void UpdateRow(int rowIndex, RowData rowData)
        {
            GridCoordinate gridCoordinate = new GridCoordinate();
            gridCoordinate.SheetId = await GetSheetId(SheetName);
            gridCoordinate.RowIndex = rowIndex;
            gridCoordinate.ColumnIndex = 0;

            Request request = new Request();
            request.UpdateCells = new UpdateCellsRequest();
            request.UpdateCells.Rows = new List<RowData> { rowData };
            request.UpdateCells.Start = gridCoordinate;
            request.UpdateCells.Fields = "*";

            BatchUpdateSpreadsheetRequest body = new BatchUpdateSpreadsheetRequest();
            body.Requests = new List<Request>() { request };
            SpreadsheetsResource.BatchUpdateRequest batchUpdateRequest = sheetsService.Spreadsheets.BatchUpdate(body, spreadsheetId);

            BatchUpdateSpreadsheetResponse response = await batchUpdateRequest.ExecuteAsync();
        }

        // TODO: Change to return data for comparison
        public async Task<int> FindRow(Dictionary<string, object> query)
        {
            SpreadsheetsResource.GetRequest getRequest = sheetsService.Spreadsheets.Get(spreadsheetId);
            getRequest.Fields = "sheets.properties,sheets.data.rowData.values.userEnteredValue";
            getRequest.Ranges = new Google.Apis.Util.Repeatable<string>(new List<string> { $"'{SheetName}'" });
            Spreadsheet spreadsheet = await getRequest.ExecuteAsync();
            IList<RowData> rows = spreadsheet.Sheets[0].Data[0].RowData;

            // rowIndex = first data row (skipping headers)
            for(int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                RowData row = rows[rowIndex];

                int matches = 0;
                foreach(KeyValuePair<string, object> pair in query)
                {
                    int colIndex = UserConfig.StatsTags.IndexOf(pair.Key);
                    
                    if(colIndex >= row.Values.Count)
                        continue;

                    ExtendedValue extendedValue = row.Values[colIndex].UserEnteredValue;
                    if(extendedValue.BoolValue != null)
                    {
                        if(extendedValue.BoolValue == (bool) pair.Value)
                            matches++;
                        else
                            continue;
                    }
                    else if(extendedValue.NumberValue != null)
                    {
                        if(extendedValue.NumberValue == Convert.ToDouble(pair.Value))
                            matches++;
                        else
                            continue;
                    }
                    else if(extendedValue.StringValue != null)
                    {
                        if(extendedValue.StringValue == pair.Value.ToString())
                            matches++;
                        else
                            continue;
                    }
                }
                
                if(matches == query.Count)
                    return rows.IndexOf(row);
            }
            return -1;
        }

        private async Task<int> GetSheetId(string name)
        {
            SpreadsheetsResource.GetRequest getRequest = sheetsService.Spreadsheets.Get(spreadsheetId);
            getRequest.Fields = "sheets.properties";
            getRequest.Ranges = new Google.Apis.Util.Repeatable<string>(new List<string> { $"'{SheetName}'" });
            Spreadsheet spreadsheet = await getRequest.ExecuteAsync();
            return (int) spreadsheet.Sheets[0].Properties.SheetId;
        }

        private async Task<Spreadsheet> CreateSpreadsheet()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.Properties = new SpreadsheetProperties() { Title = Name };
            
            spreadsheet.Sheets = new List<Sheet>() { CreateSheetTemplate() };
            
            SpreadsheetsResource.CreateRequest createRequest = sheetsService.Spreadsheets.Create(spreadsheet);
            spreadsheet = await createRequest.ExecuteAsync();
            return spreadsheet;
        }

        private async Task<string> GetSpreadsheetId()
        {
            string result = await GoogleApi.GetInstance().GetFileIdFromGoogleDrive(Name);
            return result;
        }

        private Sheet CreateSheetTemplate()
        {
            List<object> headers = UserConfig.GetSheetHeaders();

            Sheet sheet = new Sheet();
            SheetProperties properties = new SheetProperties() { Title = SheetName };
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
                rows.Add(ListToRowData(plainRow));

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

        private RowData ListToRowData(IList<object> list)
        {
            RowData row = new RowData();
            row.Values = new List<CellData>();

            foreach(object value in list)
            {
                CellData cell = new CellData();
                cell.UserEnteredValue = new ExtendedValue() { StringValue = value.ToString() };
                row.Values.Add(cell);
            }

            return row;
        }

        public string Name { get; private set; }
        public string SheetName
        {
            get => this.sheetName;
            private set
            {
                if(this.sheetName == value)
                    return;

                this.sheetId = 0;
                this.sheetName = value;
            }
        }
    }
}