using System.Linq;
using FileManagementApp.Areas.Admin.Models;
using FileManagementApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FileManagementApp.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Administrator")]
    public class ManageRoleController : Controller
    {
        private readonly UserManager<FileManagementAppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ManageRoleController> _logger;
        public ManageRoleController(UserManager<FileManagementAppUser> userManager,
            ILogger<ManageRoleController> logger,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }
        public IActionResult Index()
        {
            //Checking for status messages from add action methods
            if (TempData.ContainsKey("statusMessage"))
            {
                ViewBag.statusMessage = TempData["statusMessage"];
                TempData.Remove("statusMessage");
            }
            ViewBag.allRoles = _roleManager.Roles.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(RoleDto input)
        {
            if (ModelState.IsValid)
            {
                var existingRole = await _roleManager.FindByNameAsync(input.Name);
                if (existingRole == null)
                {
                    var role = new IdentityRole(input.Name);
                    var result = await _roleManager.CreateAsync(role);
                    TempData["statusMessage"] = result.Succeeded
                        ? $"{input.Name} has been created as a role"
                        : "Unable to create role";
                }
                else
                {
                    TempData["statusMessage"] = "Role already exist";
                }

            }

            return Redirect("/Admin/ManageRole");
        }
    }
}
