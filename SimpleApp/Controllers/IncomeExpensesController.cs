using Azure.Core;
using DocumentFormat.OpenXml.Office2010.Excel;
using ExcelDataReader;
using ExcelMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SimpleApp.BaseModel;
using SimpleApp.Constants;
using SimpleApp.Helpers;
using SimpleApp.Models;
using SimpleApp.ViewModels.IncomeExpenses;
using System.Net;

namespace SimpleApp.Controllers
{
    [Authorize]
    public class IncomeExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public IncomeExpensesController(ApplicationDbContext context)
        {
          _context = context;
        }

        #region Methods

        public ActionResult Index(int page = 1)
        {
            int pageSize = 10;

            var reportLogs = _context.IncomeExpensesLogs.Where(x => !x.IsDeleted)
                .Select(x => new LogViewModel()
                {
                    Id = x.Id,
                    CreatedBy = x.CreatedBy,
                    DateInString = x.Date.ToString("yyyy MMMM"),
                    CreatedTs = x.CreatedTs.ToString("MM/dd/yyyy h:mm tt"),
                    Date = x.Date
                }).OrderByDescending(x => x.Date).AsQueryable();

            var paginatedList = PaginatedList<LogViewModel>.Create(reportLogs, page, pageSize);

            return View(paginatedList);
        }

        /// <summary>
        /// Modal pop
        /// </summary>
        /// <returns></returns>
        public ActionResult AddOrUpdateView()
        {
            return PartialView("_AddUpdateInfoReport", new ReportViewModel() { DateString = DateTime.Now.Date.ToString("MM/yyyy") });
        }

        /// <summary>
        /// Create new report
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateView(ReportViewModel model)
        {
            var errorMessages = new List<string>();

            if (!ModelState.IsValid)
            {
                errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return Json(new { success = false, errors = errorMessages });
            }

            var _trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var reportDate = DateTime.Parse(model.DateString);

                var existedLog = await _context.IncomeExpensesLogs.AnyAsync(x => x.Date.Date == reportDate.Date && !x.IsDeleted);
                if (existedLog)
                {
                    errorMessages.Add("Report already existed for selected date");
                    return Json(new { success = false, errors = errorMessages });
                }

                var logData = new IncomeExpensesLog()
                {
                    Date = reportDate,
                    CreatedTs = DateTime.Now,
                    CreatedBy = User.Identity.Name,
                    ShowInDashboard=model.ShowInDashBoard
                };

                //remove showindashboard if check
                if (model.ShowInDashBoard)
                {
                    var existedIncomeExpenseLog = await _context.IncomeExpensesLogs
                        .ToListAsync();
                    if(existedIncomeExpenseLog!=null && existedIncomeExpenseLog.Any())
                    {
                        existedIncomeExpenseLog.ForEach(x=>x.ShowInDashboard = false);
                        await _context.SaveChangesAsync();
                    }
                }

                await _context.IncomeExpensesLogs.AddAsync(logData);
                await _context.SaveChangesAsync();


                var categories = await _context.Categories.ToListAsync();

                if (model.IsFromUpload)
                {
                    if (model.File == null)
                    {
                        errorMessages.Add("File should not be empty when checkbox is checked");
                        return Json(new { success = false, errors = errorMessages });
                    }
                    var types = new List<string> { "Income/Expenses", "Title", "Current Month Amount", "Next Month Amount" };

                    if (CsvHelpers.CheckCsvHeaders(model.File, types))
                    {
                        var excelReader = ExcelReaderFactory.CreateCsvReader(model.File.OpenReadStream());

                        using var externalImporter = new ExcelImporter(excelReader);

                        externalImporter.Configuration.RegisterClassMap<SampleCsvMap>();

                        ExcelSheet externalSheet = externalImporter.ReadSheet();

                        var incomeExpenseList = externalSheet.ReadRows<ExcelExportModel>().ToList();

                        if (incomeExpenseList != null && incomeExpenseList.Any())
                        {

                            var reportData = (from ie in incomeExpenseList
                                              join c in categories
                                              on ie.Category.ToLower().Trim() equals c.Name.ToLower().Trim()
                                              select new IncomeExpense()
                                              {
                                                  CategoryId = c.Id,
                                                  Name = ie.Title,
                                                  IncomeExpensesLogId = logData.Id,
                                                  CurrentMonthAmount = ie.CurrentMonthAmount,
                                                  NextMonthAmount = ie.NextMonthAmount
                                              }).ToList();

                            if (reportData != null)
                            {
                                await _context.IncomeExpenses.AddRangeAsync(reportData);
                            }
                            await _context.SaveChangesAsync();

                        }
                    }
                    else
                    {
                        errorMessages.Add("Invalid csv headers detected");
                        return Json(new { success = false, errors = errorMessages });
                    }
                }
                else
                {
                    var incomeExpensesSample = categories.Select(x => new IncomeExpense()
                    {
                        CategoryId = x.Id,
                        CurrentMonthAmount = 10,
                        NextMonthAmount = 10,
                        Name = "Sample",
                        IncomeExpensesLogId = logData.Id,
                    }).ToList();

                    await _context.IncomeExpenses.AddRangeAsync(incomeExpensesSample);
                    await _context.SaveChangesAsync();
                }

                await _trans.CommitAsync();

                return Json(new { success = true, id = logData.Id });

            }
            catch (Exception ex)
            {
                await _trans.RollbackAsync();
                errorMessages.Add($"Error: {ex.Message}");
                return Json(new { success = false, errors = errorMessages });
            }

        }

        public ActionResult ViewReport(int reportLogId)
        {
            return PartialView("_AddUpdateInfoReport", new ReportViewModel());
        }

        [HttpPost]
        public async Task<ActionResult> DeleteReport(int reportLogId)
        {
            var reportDetails = await _context.IncomeExpensesLogs.FirstOrDefaultAsync(x => x.Id == reportLogId);
            if (reportDetails != null) reportDetails.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Download csv sample
        /// </summary>
        /// <returns></returns>
        public ActionResult DownloadSample()
        {
            var result = new Response<MemoryStream>();
            try
            {
                var list = new List<ExcelExportModel>
            {
                new ExcelExportModel(){Category="Income",Title="Salary",CurrentMonthAmount=5000,NextMonthAmount=4000},
                new ExcelExportModel(){Category="Expenses",Title="Food",CurrentMonthAmount=3000,NextMonthAmount=5000}
            };

                var response = CsvHelpers.PrepareCSV(list);

                string filename = $"Income-Expense-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.{FileConstant.CSV_Extension}";

                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{filename}\"");

                return File(response.ToArray(), FileConstant.CSV_Content_Type, filename);
            }
            catch (Exception ex)
            {
                Request.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                result.Errors.Add(new Error()
                {
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message
                });
            }

            return Ok(result);

        }


        /// <summary>
        /// Get Report
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Report(int Id)
        {
            var result = new MainReportModel() { ReportLogId = Id };
            try
            {
                var logDetails=await _context.IncomeExpensesLogs.FirstOrDefaultAsync(x=>x.Id == Id);
                if (logDetails != null) result.ShowInDashboard=logDetails.ShowInDashboard;

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
                                NextMonthAmount = x.NextMonthAmount,
                                Name = x.Name
                            }).ToList()
                        }).ToList();

                    if (groupedList != null && groupedList.Any()) result.ReportList = groupedList;

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
            return View(result);
        }

        [HttpPost]
        public async Task<ActionResult> SaveReport([FromBody] MainReportModel model)
        {
            var _trans = await _context.Database.BeginTransactionAsync();

            try
            {

                var categories = await _context.Categories.ToListAsync();

                if (model.ReportList != null && model.ReportList.Any())
                {
                    var checkData = model.ReportList.GroupBy(x => x.Category).ToList();
                    if (checkData.Count < 2) return Json(new { success = false, errors = "Income and expenses must contain at least one data" });

                    foreach (var item in model.ReportList.Select(x => x.IncomeExpenses).ToList())
                    {
                        if (item == null || item.ToList().Count == 0)
                            return Json(new { success = false, errors = "Income and expenses must contain at least one data" });
                    }


                    var reportDataList = await _context.IncomeExpenses.Where(x => x.IncomeExpensesLogId == model.ReportLogId).ToListAsync();

                    if (reportDataList != null && reportDataList.Any()) _context.IncomeExpenses.RemoveRange(reportDataList);

                    await _context.SaveChangesAsync();

                    var newReportDataList = new List<IncomeExpense>();

                    foreach (var category in model.ReportList)
                    {
                        var currentCategory = categories.FirstOrDefault(x => x.Name.Trim().ToLower() == category.Category.Trim().ToLower());

                        if (currentCategory != null && (category.IncomeExpenses != null && category.IncomeExpenses.Any()))
                        {
                            foreach (var incomeExpense in category.IncomeExpenses)
                            {
                                newReportDataList.Add(new IncomeExpense()
                                {
                                    CategoryId = currentCategory.Id,
                                    CurrentMonthAmount = incomeExpense.CurrentMonthAmount,
                                    NextMonthAmount = incomeExpense.NextMonthAmount,
                                    Name = incomeExpense.Name,
                                    IncomeExpensesLogId = model.ReportLogId
                                });
                            }
                        }
                    }

                    if (newReportDataList.Any()) await _context.IncomeExpenses.AddRangeAsync(newReportDataList);

                    //remove showindashboard if check
                    if (model.ShowInDashboard)
                    {
                        var existedIncomeExpenseLog = await _context.IncomeExpensesLogs
                            .ToListAsync();
                        if (existedIncomeExpenseLog != null && existedIncomeExpenseLog.Any())
                        {
                            existedIncomeExpenseLog.ForEach(x => x.ShowInDashboard = false);
                            await _context.SaveChangesAsync();
                        }
                    }

                    var matchedIncomeExpenses = await _context.IncomeExpensesLogs.FirstOrDefaultAsync(x => x.Id == model.ReportLogId);
                    if (matchedIncomeExpenses != null)
                    {
                        matchedIncomeExpenses.ShowInDashboard = model.ShowInDashboard;
                    }

                    await _context.SaveChangesAsync();
                }

                await _trans.CommitAsync();
            }
            catch (Exception)
            {
                await _trans.RollbackAsync();
                return Json(new { success = false, errors = "Internal Server error" });
            }

            return Json(new { success = true });
        }

        #endregion
    }
}
