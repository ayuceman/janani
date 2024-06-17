namespace SimpleApp.ViewModels.ImageGallery
{
    public record ImageDetailsGroupedViewModel
    {
        public string ImageCategory { get; set; }
        public List<ImageDetailsViewModel> Images { get; set; }
    }
}
