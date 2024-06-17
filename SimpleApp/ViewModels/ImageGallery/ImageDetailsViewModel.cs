namespace SimpleApp.ViewModels.ImageGallery
{
    public record ImageDetailsViewModel:BaseModel
    {
        public string AlternativeText { get; set; }
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
        public int ImageCategoryId { get; set; }
        public string ImageCategory { get; set; }
        public DateTime UploadDate { get; set; }
        public IFormFile File { get; set; }
    }

    public record ImageUploadViewModel
    {
        public int? ImageCategoryId { get; set; }
        public IEnumerable<ImageDetailsViewModel> Files { get; set; }
    }
}
