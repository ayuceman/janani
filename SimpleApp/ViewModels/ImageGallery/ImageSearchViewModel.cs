namespace SimpleApp.ViewModels.ImageGallery
{
    public record ImageSearchViewModel
    {
        public int? CategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
    }
}
