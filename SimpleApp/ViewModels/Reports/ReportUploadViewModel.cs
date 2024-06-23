namespace SimpleApp.ViewModels.Reports
{
    public record ReportUploadViewModel
    {
        public IEnumerable<ReportViewModel> Files { get;set; }
    }
}
