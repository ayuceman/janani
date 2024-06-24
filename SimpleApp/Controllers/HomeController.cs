using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleApp.BaseModel;
using SimpleApp.Models;
using SimpleApp.ViewModels.Common;
using SimpleApp.ViewModels.ImageGallery;
using SimpleApp.ViewModels.IncomeExpenses;
using System.Diagnostics;

namespace SimpleApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SimpleAppContext _context;
        public HomeController(ILogger<HomeController> logger, SimpleAppContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(ImageSearchViewModel filterModel)
        {
            var model = new CommonModel();

            
            #region Gallery

            var imageList = _context.ImageDetails
                .Include(x => x.ImageCategory).Where(x => !x.ImageCategory.IsDeleted)
                .Select(x => new ImageDetailsViewModel()
                {
                    Id = x.Id,
                    AlternativeText = x.AlternativeText,
                    ImageName = x.ImageName,
                    ImagePath = Path.Combine("/uploads", x.ImageName),
                    ImageCategory = x.ImageCategory.Name,
                    ImageCategoryId = x.ImageCategoryId,
                    UploadDate = x.UploadedTs
                }).OrderByDescending(x => x.Id).AsQueryable();

            model.ImageGallery = PaginatedList<ImageDetailsViewModel>.Create(imageList, filterModel.Page, 12);

            #endregion

            #region Income Expenses

            
            var logDetails = await _context.IncomeExpensesLogs.FirstOrDefaultAsync();

            if(logDetails != null)
            {
                var incomeExpenseList = await _context.IncomeExpenses
             .Include(x => x.IncomeExpensesLog)
             .Include(x => x.Category)
             .Where(x => x.IncomeExpensesLogId == logDetails.Id && !x.IncomeExpensesLog.IsDeleted)
             .ToListAsync();

                if (incomeExpenseList != null && incomeExpenseList.Any())
                {
                    var date = incomeExpenseList.FirstOrDefault()?.IncomeExpensesLog.Date;

                    model.ReportModel.DateString = date?.ToString("yyyy MMMM");

                    var groupedList = incomeExpenseList.GroupBy(x => new { x.Category.Name, x.Category.Id })
                        .Select(x => new GroupIncomExpensesModel()
                        {
                            Category = x.Key.Name,
                            CategoryId = x.Key.Id,
                            IncomeExpenses = x.Select(x => new IncomeExpenseModel()
                            {
                                CategoryId = x.CategoryId,
                                CurrentMonthAmount = x.CurrentMonthAmount,
                                Name = x.Name
                            }).ToList()
                        }).ToList();

                    if (groupedList != null && groupedList.Any())
                    {
                        model.ReportModel.ReportList = groupedList;
                    }

                }
            }
         



            #endregion

            return View(model);

        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
