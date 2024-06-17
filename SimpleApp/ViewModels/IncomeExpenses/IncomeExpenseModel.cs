namespace SimpleApp.ViewModels.IncomeExpenses
{
    public record IncomeExpenseModel:BaseModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? CurrentMonthAmount { get; set; }
        public decimal? NextMonthAmount { get; set; }
    }
}
