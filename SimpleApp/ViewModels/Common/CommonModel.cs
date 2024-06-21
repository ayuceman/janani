using SimpleApp.BaseModel;
using SimpleApp.ViewModels.ImageGallery;
using SimpleApp.ViewModels.IncomeExpenses;

namespace SimpleApp.ViewModels.Common
{
    public record CommonModel
    {
        public CommonModel()
        {
            ReportModel = new MainReportModel();
        }
        public MainReportModel ReportModel { get; set; }
        public PaginatedList<ImageDetailsViewModel> ImageGallery { get; set; }

    }
}
