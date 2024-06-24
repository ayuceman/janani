using System;
using System.Collections.Generic;

namespace SimpleApp.Models;

public partial class IncomeExpensesLog
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public DateTime CreatedTs { get; set; }

    public string CreatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<IncomeExpense> IncomeExpenses { get; set; } = new List<IncomeExpense>();
}
