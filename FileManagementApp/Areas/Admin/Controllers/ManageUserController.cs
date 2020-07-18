using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileManagementApp.Areas.Admin.Models;
using FileManagementApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Linq;
using FileManagementApp.Areas.Identity.Data.Repo;

namespace FileManagementApp.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Administrator")]
    public class ManageUserController : Controller
    {
        private readonly UserManager<FileManagementAppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ManageUserController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUserRepository _userRepository;
        public ManageUserController(UserManager<FileManagementAppUser> userManager,
            ILogger<ManageUserController> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IUserRepository userRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            var allUsers = _userRepository.GetUserListWithRoles();
            return View(allUsers);
        }

        public IActionResult Add()
        {
            ViewBag.roles = _roleManager.Roles.Select(r => r).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddUserModel input)
        {
            if (ModelState.IsValid)
            {
                var user = new FileManagementAppUser { UserName = input.Email, Email = input.Email };
                var result = await _userManager.CreateAsync(user, input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("New user added with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                      "/Account/ConfirmEmail",
                       pageHandler: null,
                      values: new { area = "Identity", userId = user.Id, code },
                      protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(input.Email, "Confirm your email",
                       $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await _userManager.AddToRoleAsync(user, input.Role);

                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewBag.roles = _roleManager.Roles.Select(r => r).ToList();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute]string id)
        {
            //Checking if user Id is provided
            if (String.IsNullOrWhiteSpace(id))
            {
                return Redirect("/Admin/manageuser/Index");
            }

            //Checking for status messages from other action methods
            if (TempData.ContainsKey("statusMessage"))
            {
                ViewBag.statusMessage = TempData["statusMessage"];
                TempData.Remove("statusMessage");
            }

            var user = await _userRepository.GetUserWithRoles(id);
            ViewBag.user = user;
            if (user != null)
            {
                var roles = _roleManager.Roles.ToList()
                                                         .Where(r => user.RoleNames
                                                         .All(ur => ur != r.Name)).ToList();
                ViewBag.availableRoles = roles;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ManageUserModel manageUserModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(manageUserModel.EditUserModel.Id);
                user.PhoneNumber = manageUserModel.EditUserModel.PhoneNumber;
                _userRepository.Update(user);
                bool updated = await _userRepository.SaveChanges();
                ViewBag.statusMessage = updated ? "User profile updated" : "Unable to update changes";
            }

            return await Edit(manageUserModel.EditUserModel.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToRole(ManageUserModel manageUserModel)
        {
            string userId = manageUserModel.AddUserToRoleModel.Id;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(manageUserModel.AddUserToRoleModel.Id);
                var result = await _userManager.AddToRoleAsync(user, manageUserModel.AddUserToRoleModel.Role);
                TempData["statusMessage"] = result.Succeeded ? $"User has been added to {manageUserModel.AddUserToRoleModel.Role} role" : "Unable to add user to role";
            }

            return Redirect($"/Admin/ManageUser/Edit/{userId}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromRole(ManageUserModel manageUserModel)
        {
            string userId = manageUserModel.RemoveUserFromRoleModel.Id;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(manageUserModel.RemoveUserFromRoleModel.Id);
                var result = await _userManager.RemoveFromRoleAsync(user, manageUserModel.RemoveUserFromRoleModel.Role);
                TempData["statusMessage"] = result.Succeeded
                    ? $"User has been removed from {manageUserModel.RemoveUserFromRoleModel.Role} role"
                    : "Unable to remove user from role";
            }

            return Redirect($"/Admin/ManageUser/Edit/{userId}");
        }
    }
}
