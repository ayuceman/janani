using System.ComponentModel.DataAnnotations;

namespace SimpleApp.ViewModels.Reports
{
    public record ReportUploadViewModel
    {
        [Required(ErrorMessage = "Pdf file cannot be empty")]
        public IEnumerable<ReportFileViewModel> Files { get;set; }
    }

    public record ReportFileViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedTs { get; set; }
        public IFormFile File { get; set; }
        public string FilePath { get; set; }
    }
}
