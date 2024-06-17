using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleApp.BaseModel;
using SimpleApp.Models;
using SimpleApp.ViewModels.ImageGallery;
using SimpleApp.ViewModels.IncomeExpenses;

namespace SimpleApp.Controllers
{
    [Authorize]
    public class ImageCategoryController: Controller
    {
        private readonly ApplicationDbContext _context;

        public ImageCategoryController(ApplicationDbContext context)
        {
          _context = context;
        }
        public IActionResult Index(int page=1)
        {
            int pageSize = 10;

            var categories = _context.ImageCategories.Where(x=>!x.IsDefault && !x.IsDeleted)
                .Select(x => new ImageCategoryModel()
                {
                    Id = x.Id,
                    CreatedBy = x.CreatedBy,
                    Description = x.Description,
                    Name = x.Name
                }).OrderByDescending(x => x.Id).AsQueryable();

            var paginatedList = PaginatedList<ImageCategoryModel>.Create(categories, page, pageSize);

            return View(paginatedList);
        }


        public ActionResult AddCategory()
        {
            return PartialView("_AddCategory", new ImageCategoryModel() );
        }

        [HttpPost]
        public async Task<ActionResult> AddCategory([FromBody]ImageCategoryModel model)
        {
            var errorMessages = new List<string>();
            try
            {
                if (!ModelState.IsValid)
                {
                    errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                    return Json(new { success = false, errors = errorMessages });
                }

                var existedNameCategory = await _context.ImageCategories
                    .FirstOrDefaultAsync(x => x.Name.Trim().ToLower() == model.Name.Trim().ToLower() && !x.IsDeleted);

                if (existedNameCategory != null)
                {
                    if (existedNameCategory.IsDefault)
                    {
                        errorMessages.Add("Default category existed with same category name, please select different name");
                    }
                    else
                    {
                        errorMessages.Add("Category name existed, please select different name");
                    }

                    return Json(new { success = false, errors = errorMessages });
                }

                var categoryToAdd = new ImageCategory()
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreatedBy = User.Identity.Name
                };
                await _context.ImageCategories.AddAsync(categoryToAdd);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Error: {ex.Message}");
                return Json(new { success = false, errors = errorMessages });
            }
        }


        public async Task<ActionResult> GetCategory(int id)
        {
            var categoryDetails = await _context.ImageCategories.Where(x => x.Id == id && !x.IsDeleted && !x.IsDefault)
                .Select(x => new ImageCategoryModel()
                {
                    Id = x.Id,
                    Description = x.Description,
                    Name = x.Name
                }).FirstOrDefaultAsync();

            return PartialView("_UpdateCategory", categoryDetails);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCategory([FromBody]ImageCategoryModel model)
        {
            var errorMessages = new List<string>();
            try
            {
                if (!ModelState.IsValid)
                {
                    errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                    return Json(new { success = false, errors = errorMessages });
                }

                var categoryList = await _context.ImageCategories.ToListAsync();

                var categoryDetails= categoryList.FirstOrDefault(x => x.Id == model.Id && !x.IsDeleted && !x.IsDefault);

                if (categoryDetails != null)
                {
                    if (categoryDetails.Name.Trim().ToLower() != model.Name.Trim().ToLower())
                    {
                        var existedName=categoryList.Any(x=>x.Name.Trim().ToLower()==model.Name.Trim().ToLower());
                        if (existedName)
                        {
                            errorMessages.Add("Name already used by different category, pleas select different name");
                            return Json(new { success = false, errors = errorMessages });
                        }
                        else
                        {
                            categoryDetails.Name= model.Name;
                        }
                    }
                    categoryDetails.Description = model.Description;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    errorMessages.Add("Invalid category detected");
                    return Json(new { success = false, errors = errorMessages });
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Error: {ex.Message}");
                return Json(new { success = false, errors = errorMessages });
            }
        }


        [HttpPost]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var imageCategory = await _context.ImageCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (imageCategory != null) imageCategory.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
