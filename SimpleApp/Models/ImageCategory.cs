using System;
using System.Collections.Generic;

namespace SimpleApp.Models;

public partial class ImageCategory
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public bool IsDefault { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ImageDetail> ImageDetails { get; set; } = new List<ImageDetail>();
}
