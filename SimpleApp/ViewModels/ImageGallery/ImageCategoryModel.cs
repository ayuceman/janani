using System.ComponentModel.DataAnnotations;

namespace SimpleApp.ViewModels.ImageGallery
{
    public record ImageCategoryModel:BaseModel
    {
        [Required(ErrorMessage = "Category name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category description is required")]
        public string Description { get; set; }

        public string CreatedBy { get; set; }

        public bool IsDefault { get; set; }
    }
}
