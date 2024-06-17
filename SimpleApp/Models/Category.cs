using System;
using System.Collections.Generic;

namespace SimpleApp.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<IncomeExpense> IncomeExpenses { get; set; } = new List<IncomeExpense>();
}
