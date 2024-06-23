using System;
using System.Collections.Generic;

namespace SimpleApp.Models;

public partial class IncomeExpense
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int CategoryId { get; set; }

    public int IncomeExpensesLogId { get; set; }

    public decimal? CurrentMonthAmount { get; set; }

    public virtual Category Category { get; set; }

    public virtual IncomeExpensesLog IncomeExpensesLog { get; set; }
}
