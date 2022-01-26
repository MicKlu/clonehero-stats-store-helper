using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace StatsStoreHelper.Apis.GoogleApi
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
    
        public async Task Init(UserCredential credentials, string sheetName)
        {
            if(this.credentials != credentials)
            {
                this.credentials = credentials;

                System.Console.WriteLine("Initializing SheetsService");
                sheetsService = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = PluginInfo.PLUGIN_NAME
                });

                this.spreadsheetId = null;
                this.sheetId = 0;
            }

            if(this.spreadsheetId == null)
            {
                this.SheetName = sheetName;
                
                if(UserConfig.SpreadsheetId.Length != 0 && await SpreadsheetExists(UserConfig.SpreadsheetId))
                    this.spreadsheetId = UserConfig.SpreadsheetId;
                // else
                    // TODO: Show error if wrong

                if(this.spreadsheetId == null)
                {
                    Spreadsheet spreadsheet = await CreateSpreadsheet();
                    this.spreadsheetId = spreadsheet.SpreadsheetId;
                    this.sheetId = (int) spreadsheet.Sheets[0].Properties.SheetId;
                    UserConfig.SpreadsheetId = spreadsheet.SpreadsheetId;
                }
            }

            if(this.SheetName != sheetName)
                this.sheetId = 0;
            
            if(this.sheetId == 0)
            {
                this.SheetName = sheetName;
                this.sheetId = await GetSheetId(sheetName);
                if(this.sheetId == 0)
                    this.sheetId = await AddSheet(CreateSheetTemplate());
            }
        }

        private async Task<int> AddSheet(Sheet sheet)
        {
            Request request = new Request();
            request.AddSheet = new AddSheetRequest();
            request.AddSheet.Properties = new SheetProperties();
            request.AddSheet.Properties.Title = sheet.Properties.Title;
            
            BatchUpdateSpreadsheetRequest body = new BatchUpdateSpreadsheetRequest();
            body.Requests = new List<Request>() { request };
            SpreadsheetsResource.BatchUpdateRequest batchUpdateRequest = sheetsService.Spreadsheets.BatchUpdate(body, spreadsheetId);

            BatchUpdateSpreadsheetResponse response = await batchUpdateRequest.ExecuteAsync();
            int sheetId = (int) response.Replies[0].AddSheet.Properties.SheetId;
            
            request = new Request();
            request.AppendCells = new AppendCellsRequest();
            request.AppendCells.SheetId = sheetId;
            request.AppendCells.Rows = sheet.Data[0].RowData;
            request.AppendCells.Fields = "*";

            body = new BatchUpdateSpreadsheetRequest();
            body.Requests = new List<Request>() { request };
            batchUpdateRequest = sheetsService.Spreadsheets.BatchUpdate(body, spreadsheetId);

            response = await batchUpdateRequest.ExecuteAsync();

            return sheetId;
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

        public async Task<FindRowResult> FindRow(Dictionary<string, object> query)
        {
            FindRowResult result = new FindRowResult();

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
                    int colIndex = UserConfig.UserStatsTags.IndexOf(pair.Key);
                    
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
                {
                    result.Index = rows.IndexOf(row);
                    result.RowData = row;
                }
                    
            }
            return result;
        }

        private async Task<int> GetSheetId(string name)
        {
            SpreadsheetsResource.GetRequest getRequest = sheetsService.Spreadsheets.Get(spreadsheetId);
            getRequest.Fields = "sheets.properties";
            getRequest.Ranges = new Google.Apis.Util.Repeatable<string>(new List<string> { $"'{SheetName}'" });
            
            try
            {
                Spreadsheet spreadsheet = await getRequest.ExecuteAsync();
                return (int) spreadsheet.Sheets[0].Properties.SheetId;
            }
            catch(Google.GoogleApiException e)
            {   
                JObject error = JObject.Parse(e.Error.ErrorResponseContent);
                if((string) error["error"]["status"] == "INVALID_ARGUMENT")
                    return 0;

                throw e;
            }
        }

        private async Task<bool> SpreadsheetExists(string spreadsheetId)
        {
            SpreadsheetsResource.GetRequest getRequest = sheetsService.Spreadsheets.Get(spreadsheetId);
            try
            {
                Spreadsheet spreadsheet = await getRequest.ExecuteAsync();
                return true;
            }
            catch(Google.GoogleApiException e)
            {   
                JObject error = JObject.Parse(e.Error.ErrorResponseContent);
                if((string) error["error"]["status"] == "NOT_FOUND")
                    return false;

                throw e;
            }
        }

        private async Task<Spreadsheet> CreateSpreadsheet()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.Properties = new SpreadsheetProperties() { Title = PluginInfo.PLUGIN_NAME };
            
            spreadsheet.Sheets = new List<Sheet>() { CreateSheetTemplate() };
            
            SpreadsheetsResource.CreateRequest createRequest = sheetsService.Spreadsheets.Create(spreadsheet);
            spreadsheet = await createRequest.ExecuteAsync();
            return spreadsheet;
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