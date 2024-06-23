using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleApp.BaseModel;
using SimpleApp.Models;
using SimpleApp.ViewModels.ImageGallery;

namespace SimpleApp.Controllers
{
    [Authorize]
    public class ImageGalleryController : Controller
    {
        private readonly SimpleAppContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ImageGalleryController(SimpleAppContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostEnvironment;
        }
        public async Task<IActionResult> Index(ImageSearchViewModel model)
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
                imageList = imageList.Where(x => x.UploadDate.Date >= model.FromDate.Value.Date && x.UploadDate <= model.ToDate.Value.Date);
            }
            #endregion

            var paginatedList = PaginatedList<ImageDetailsViewModel>.Create(imageList, model.Page, pageSize);

            var categories = await _context.ImageCategories.Where(x => !x.IsDeleted).ToListAsync();

            var categoryList = new SelectList(categories, "Id", "Name");

            ViewBag.CategoryList = categoryList;

            return View(paginatedList);
        }

        public async Task<IActionResult> SearchIndex(ImageSearchViewModel model)
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
                imageList = imageList.Where(x => x.UploadDate.Date >= model.FromDate.Value.Date && x.UploadDate <= model.ToDate.Value.Date);
            }
            #endregion

            var paginatedList = PaginatedList<ImageDetailsViewModel>.Create(imageList, model.Page, pageSize);

            var categories = await _context.ImageCategories.Where(x => !x.IsDeleted).ToListAsync();

            var categoryList = new SelectList(categories, "Id", "Name");

            ViewBag.CategoryList = categoryList;

            return PartialView("_ImageGalleryList", paginatedList);
        }

        public IActionResult ImageDetailsList(int page = 1)
        {
            int pageSize = 10;

            var imageList = _context.ImageDetails
                .Include(x => x.ImageCategory).Where(x => !x.ImageCategory.IsDeleted)
                .Select(x => new ImageDetailsViewModel()
                {
                    Id = x.Id,
                    AlternativeText = x.AlternativeText,
                    ImageName = x.ImageName,
                    ImagePath = Path.Combine("/uploads", x.ImageName),
                    ImageCategory = x.ImageCategory.Name
                }).OrderByDescending(x => x.Id).AsQueryable();

            var paginatedList = PaginatedList<ImageDetailsViewModel>.Create(imageList, page, pageSize);

            return View("_ImageSettings", paginatedList);
        }

        public async Task<ActionResult> UploadImage()
        {
            var categories = await _context.ImageCategories.Where(x => !x.IsDeleted && !x.IsDefault).ToListAsync();

            var categoryList = new SelectList(categories, "Id", "Name");

            // Assign SelectList to ViewBag or ViewData (preferably use ViewBag)
            ViewBag.CategoryList = categoryList;

            return PartialView("_UploadImage");
        }

        [HttpPost]
        public async Task<ActionResult> UploadImage(ImageUploadViewModel model)
        {
            var _trans = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model != null)
                {
                    int categoryId = 0;

                    var categoryList = await _context.ImageCategories.ToListAsync();

                    var categoryDetails = categoryList.FirstOrDefault(x => x.Id == model.ImageCategoryId);

                    if (categoryDetails == null)
                    {
                        var defaultCategory = categoryList.FirstOrDefault(x => x.IsDefault);
                        if (defaultCategory != null) categoryId = defaultCategory.Id;
                        else
                        {
                            return Json(new { success = false, errors = "Default Category is empty" });
                        }
                    }
                    else
                    {
                        categoryId = categoryDetails.Id;
                    }

                    if (model.Files != null && model.Files.Any())
                    {
                        // Specify your upload directory
                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

                        // Ensure the directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filesToAdd = new List<ImageDetail>();

                        foreach (var file in model.Files)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.File.FileName;

                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.File.CopyToAsync(stream);
                            }
                            filesToAdd.Add(new ImageDetail()
                            {
                                AlternativeText = file.AlternativeText,
                                ImageCategoryId = categoryId,
                                ImageName = uniqueFileName,
                                UploadedTs = DateTime.Now,
                            });
                        }

                        if (filesToAdd.Any())
                        {
                            await _context.ImageDetails.AddRangeAsync(filesToAdd);
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
            catch (Exception)
            {
                await _trans.RollbackAsync();
                return Json(new { success = false, errors = "Internal Server Error" });
            }

        }

        [HttpPost]
        public async Task<ActionResult> DeleteImage(int id)
        {
            var imageDetails = await _context.ImageDetails.FirstOrDefaultAsync(x => x.Id == id);
            if (imageDetails != null)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

                string filePath = Path.Combine(uploadsFolder, imageDetails.ImageName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.ImageDetails.Remove(imageDetails);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

    }
}

