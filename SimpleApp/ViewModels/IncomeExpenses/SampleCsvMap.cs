using ExcelMapper;

namespace SimpleApp.ViewModels.IncomeExpenses
{
    public class SampleCsvMap: ExcelClassMap<ExcelExportModel>
    {
        public SampleCsvMap()
        {
            Map(x => x.Category).WithColumnName("Income/Expenses").MakeOptional();
            Map(x => x.Title).WithColumnName("Title").MakeOptional();
            Map(x => x.CurrentMonthAmount).WithColumnName("Current Month Amount").MakeOptional();
        }
    }
}
