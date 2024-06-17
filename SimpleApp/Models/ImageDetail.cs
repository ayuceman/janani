using System;
using System.Collections.Generic;

namespace SimpleApp.Models;

public partial class ImageDetail
{
    public int Id { get; set; }

    public string AlternativeText { get; set; }

    public int ImageCategoryId { get; set; }

    public string ImageName { get; set; }

    public DateTime UploadedTs { get; set; }

    public virtual ImageCategory ImageCategory { get; set; }
}
