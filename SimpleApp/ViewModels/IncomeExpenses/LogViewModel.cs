namespace SimpleApp.ViewModels.IncomeExpenses
{
    public record LogViewModel : BaseModel
    {
        public string DateInString { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedTs { get; set; }
        public DateTime Date { get; set; }
    }
}
