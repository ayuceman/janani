namespace SimpleApp.ViewModels.IncomeExpenses
{
    public record GroupIncomExpensesModel:BaseModel
    {
        public GroupIncomExpensesModel()
        {
            IncomeExpenses = new List<IncomeExpenseModel>();
        }
        public string Category { get; set; }
        public int CategoryId {  get; set; }
        public IEnumerable<IncomeExpenseModel> IncomeExpenses { get; set; }
    }

    public record MainReportModel
    {
        public MainReportModel()
        {
            ReportList = new List<GroupIncomExpensesModel>();
        }
        public int ReportLogId { get; set; }
        public string DateString { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<GroupIncomExpensesModel> ReportList { get; set; }
        public bool ShowInDashboard { get; set; }
    }
}
