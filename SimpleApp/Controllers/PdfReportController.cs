using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleApp.BaseModel;
using SimpleApp.Models;
using SimpleApp.ViewModels.ImageGallery;
using SimpleApp.ViewModels.Reports;

namespace SimpleApp.Controllers
{
    [Authorize]
    public class PdfReportController : Controller
    {
        private readonly SimpleAppContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PdfReportController(SimpleAppContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostEnvironment;
        }
        public IActionResult Index(SearchViewModel model)
        {
            int pageSize = 12;

            var fileList = _context.ReportFiles
                .Select(x => new ReportFileViewModel()
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    Name = x.Name,
                    UploadedTs = x.UploadedTs,
                    FilePath = Path.Combine("/reports", x.FileName),
                }).OrderByDescending(x => x.Id).AsQueryable();

            var paginatedList = PaginatedList<ReportFileViewModel>.Create(fileList, model.Page, pageSize);

            return View(paginatedList);
        }

        public ActionResult UploadFile()
        {

            return PartialView("_UploadFile");
        }

        [HttpPost]
        public async Task<ActionResult> UploadFile(ReportUploadViewModel model)
        {
            var _trans = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model != null)
                {
                    if (model.Files != null && model.Files.Any())
                    {
                        // Specify your upload directory
                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "reports");

                        // Ensure the directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filesToAdd = new List<ReportFile>();

                        foreach (var file in model.Files)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.File.FileName;

                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.File.CopyToAsync(stream);
                            }
                            filesToAdd.Add(new ReportFile()
                            {
                                FileName = uniqueFileName,
                                Name = file.Name,
                                UploadedTs = DateTime.Now,
                            });
                        }

                        if (filesToAdd.Any())
                        {
                            await _context.ReportFiles.AddRangeAsync(filesToAdd);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        return Json(new { success = false, errors = "Files are empty" });
                    }
                }

                await _trans.CommitAsync();

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                await _trans.RollbackAsync();
                return Json(new { success = false, errors = "Internal Server Error" });
            }

        }

        [HttpPost]
        public async Task<ActionResult> DeleteFile(int id)
        {
            var fileDetails = await _context.ReportFiles.FirstOrDefaultAsync(x => x.Id == id);
            if (fileDetails != null)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "reports");

                string filePath = Path.Combine(uploadsFolder, fileDetails.FileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.ReportFiles.Remove(fileDetails);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var report = await _context.ReportFiles.FirstOrDefaultAsync(x=>x.Id==id);
            if (report == null)
            {
                return NotFound();
            }

            string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "reports");

            string filePath = Path.Combine(uploadsFolder, report.FileName);

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var contentType = "application/pdf";
            var fileName = Path.GetFileName(filePath);

            return File(memory, contentType, fileName);
        }


        [AllowAnonymous]
        public async Task<IActionResult> ReportList()
        {
            var fileList =await _context.ReportFiles
                .Select(x => new ReportFileViewModel()
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    Name = x.Name,
                    UploadedTs = x.UploadedTs,
                    FilePath = Path.Combine("/reports", x.FileName),
                }).OrderByDescending(x => x.Id).ToListAsync();

            return Json(fileList);
        }
    }
}

