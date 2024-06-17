using ExcelMapper;

namespace SimpleApp.ViewModels.IncomeExpenses
{
    public class ExcelExportModel
    {
        [ExcelColumnName("Income/Expenses")]
        public string Category { get; set; }
        public string Title { get; set; }

        [ExcelColumnName("Current Month Amount")]
        public decimal? CurrentMonthAmount { get; set; }

        [ExcelColumnName("Next Month Amount")]
        public decimal? NextMonthAmount { get; set; }
    }
}
