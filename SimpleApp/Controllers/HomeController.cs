using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
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

            var categories = await _context.ImageCategories.Where(x => !x.IsDeleted).ToListAsync();

            var categoryList = new SelectList(categories, "Id", "Name");

            ViewBag.CategoryList = categoryList;

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

        public async Task<ActionResult> ViewReportById(int Id)
        {
            var result = new MainReportModel() { ReportLogId = Id };
            try
            {
                var incomeExpenseList = await _context.IncomeExpenses
                    .Include(x => x.IncomeExpensesLog)
                    .Include(x => x.Category)
                    .Where(x => x.IncomeExpensesLogId == Id && !x.IncomeExpensesLog.IsDeleted)
                    .ToListAsync();

                if (incomeExpenseList != null && incomeExpenseList.Any())
                {
                    var date = incomeExpenseList.FirstOrDefault()?.IncomeExpensesLog.Date;

                    result.DateString = date?.ToString("yyyy MMMM");

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
                            ,Total=x.Sum(y=>y.CurrentMonthAmount)
                        }).ToList();

                    if (groupedList != null && groupedList.Any())
                    {
                        decimal? Total = 0;

                        foreach (var group in groupedList)
                        {
                            if(group.Category=="Income") Total = group.Total;
                            else Total-=group.Total;
                        }
                        result.ReportList = groupedList;
                        result.Total = Total;
                    }

                }
                else
                {
                    result.ErrorMessage = "Data is empty";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error:{ex.Message}";
            }
            return PartialView("_ViewInfoReport", result);
        }


        public IActionResult SearchIndex(ImageSearchViewModel model)
        {
            int pageSize = 12;

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

            #region Filters

            if (model.CategoryId != null && model.CategoryId > 0)
            {
                imageList = imageList.Where(x => x.ImageCategoryId == model.CategoryId);
            }

            if (model.FromDate != null && model.ToDate != null)
            {
                model.FromDate = model.FromDate.Value.AddDays(-1);
                model.ToDate = model.ToDate.Value.AddDays(1);
                imageList = imageList.Where(x => x.UploadDate.Date >= model.FromDate.Value.Date && x.UploadDate <= model.ToDate.Value.Date);
            }
            #endregion

            var paginatedList = PaginatedList<ImageDetailsViewModel>.Create(imageList, model.Page, pageSize);

            return PartialView("_ViewGallery", paginatedList);
        }
    }
}
