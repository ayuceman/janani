using Azure;
using ClosedXML.Excel;
using CsvHelper;
using ExcelMapper;
using System.ComponentModel;
using System.Data;
using System.Formats.Asn1;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SimpleApp.Helpers
{
    public static class CsvHelpers
    {
        public static MemoryStream PrepareCSV<T>(List<T> list)
        {
            var result = new MemoryStream();
            try
            {
                using var workbook = new XLWorkbook();
                var categorysheet = workbook.Worksheets.Add();
                var rows = ListtoDataTableConverter.ToDataTable(list);
                categorysheet.FirstCell().InsertTable(rows, true);
                categorysheet.FirstRow().Cells(usedCellsOnly: true).ToList().ForEach(x =>
                {
                    x.Style.Font.Bold = true;
                });
                categorysheet.Columns().AdjustToContents();

                using (var csvStream = new MemoryStream())
                {
                    categorysheet.RangeUsed().Rows().Select(row => string.Join(",", row.Cells().Select(cell => cell.Value.ToString())))
                        .ToList()
                        .ForEach(rowData =>
                        {
                            var csvBytes = Encoding.UTF8.GetBytes(rowData + Environment.NewLine);
                            csvStream.Write(csvBytes, 0, csvBytes.Length);
                        });

                    csvStream.Flush();
                    csvStream.Position = 0;
                    result = csvStream;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static class ListtoDataTableConverter
        {
            public static DataTable ToDataTable<T>(List<T> items)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
                DataTable table = new DataTable("table", "table");
                foreach (PropertyDescriptor prop in properties)
                {
                    string columnname = prop.Attributes.OfType<ExcelColumnNameAttribute>().FirstOrDefault()?.Name ?? prop.Name;
                    if (!string.IsNullOrEmpty(columnname))
                    {
                        table.Columns.Add(columnname, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }
                    else
                    {
                        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }
                }

                foreach (T item in items)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        string columnName = prop.Attributes.OfType<ExcelColumnNameAttribute>().FirstOrDefault()?.Name ?? prop.Name;
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            row[columnName] = prop.GetValue(item) ?? DBNull.Value;
                        }
                        else
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }

                    }
                    table.Rows.Add(row);
                }
                //put a breakpoint here and check datatable
                return table;
            }
        }

        public static bool CheckCsvHeaders(IFormFile file, List<string> categoryTypes)
        {
            List<string> csvheaders;
            using (var stream = file.OpenReadStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        var headers = csv.HeaderRecord;
                        csvheaders = headers.ToList();
                    }
                }
            }
            List<string> list = new List<string>();

            if (csvheaders.Any())
            {
                foreach (var header in csvheaders)
                {
                    list.Add(header);
                }
            }
            return categoryTypes.All(x => list.Contains(x));
        }
    }
}
