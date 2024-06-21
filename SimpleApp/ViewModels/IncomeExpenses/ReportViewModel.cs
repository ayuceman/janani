using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SimpleApp.ViewModels.IncomeExpenses
{
    public class ReportViewModel
    {
        public bool IsFromUpload { get; set; }
        public IFormFile File { get; set;}
        public int ReportLogId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public string DateString { get; set; }
        public bool ShowInDashBoard { get; set; }
    }
}
