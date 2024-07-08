using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SimpleApp.Areas.Identity.Pages.Account;
using SimpleApp.BaseModel;
using SimpleApp.Models;
using SimpleApp.ViewModels.Account;
using SimpleApp.ViewModels.Common;
using SimpleApp.ViewModels.ImageGallery;
using SimpleApp.ViewModels.IncomeExpenses;
using System.Diagnostics;
using System.Security.Claims;
namespace SimpleApp.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly SimpleAppContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AccountController(ILogger<AccountController> logger,
            SimpleAppContext context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public  ActionResult Index(int page=1)
        {
            int pageSize = 10;

            var users = _context.AspNetUsers.Where(x=>x.UserName.Trim().ToLower()!=HttpContext.User.Identity.Name.Trim().ToLower())
                .Select(x => new UserModel()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email
                }).OrderByDescending(x => x.Id).AsQueryable();

            var paginatedList = PaginatedList<UserModel>.Create(users, page, pageSize);

            return View(paginatedList);
        }


        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            var userDetails = await _context.AspNetUsers.FirstOrDefaultAsync(x => x.Id == id && x.UserName.Trim().ToLower() != HttpContext.User.Identity.Name.Trim().ToLower());
            if (userDetails != null) 
            {
                 _context.AspNetUsers.Remove(userDetails);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        public  ActionResult AddUser()
        {
            return PartialView("AddUser", new RegisterViewModel());
        }

        [HttpPost]
        public async Task<ActionResult> AddUser([FromBody] RegisterViewModel model)
        {
            var errorMessages = new List<string>();
            try
            {
                if (!ModelState.IsValid)
                {
                    errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                    return Json(new { success = false, errors = errorMessages });
                }

                var user = new IdentityUser { UserName = model.Email, Email = model.Email,EmailConfirmed=true };

                var result = await _userManager.CreateAsync(user,model.Password);

                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }
                else
                {
                    errorMessages.Add(result.Errors.First().Description);
                    return Json(new { success = false, errors = errorMessages });
                }
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Error: {ex.Message}");
                return Json(new { success = false, errors = errorMessages });
            }
        }


        public async Task<ActionResult> Reset(string id)
        {
            var details=await _userManager.FindByIdAsync(id);

            return PartialView("ResetUserPassword", new RegisterViewModel() { Email=details.Email});
        }

        [HttpPost]
        public async Task<ActionResult> Reset([FromBody] RegisterViewModel model)
        {
            var errorMessages = new List<string>();
            try
            {
                if (!ModelState.IsValid)
                {
                    errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                    return Json(new { success = false, errors = errorMessages });
                }

                var existedUser = await _context.AspNetUsers.FirstOrDefaultAsync(x=>x.Id == model.Id);

                var user = await _userManager.FindByIdAsync(model.Id);

                if(existedUser != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

                    if (result.Succeeded)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        errorMessages.Add(result.Errors.First().Description);
                        return Json(new { success = false, errors = errorMessages });
                    }
                }
                else
                {
                    errorMessages.Add($"Invalid user detected!");
                    return Json(new { success = false, errors = errorMessages });
                }
               
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Error: {ex.Message}");
                return Json(new { success = false, errors = errorMessages });
            }
        }


    }
}
