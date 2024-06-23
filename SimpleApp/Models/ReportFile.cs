using System;
using System.Collections.Generic;

namespace SimpleApp.Models;

public partial class ReportFile
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string FileName { get; set; }

    public DateTime UploadedTs { get; set; }
}
