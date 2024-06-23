namespace SimpleApp.ViewModels.Reports
{
    public record ReportViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FileName { get; set; }

        public DateTime UploadedTs { get; set; }
        public IFormFile File { get; set; }
        public string FilePath { get; set; }
    }
}
