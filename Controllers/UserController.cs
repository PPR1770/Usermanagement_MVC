using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApp.Models;

namespace ChatApp.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var users = _userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                users = users.Where(u =>
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                    u.Email!.Contains(search));

            ViewBag.Search = search;
            return View(await users.OrderBy(u => u.FirstName).ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.AllRoles = allRoles;
            ViewBag.UserRoles = userRoles;
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string firstName, string lastName,
            string phoneNumber, bool isActive, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;
            user.IsActive = isActive;
            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (roles != null && roles.Any())
                await _userManager.AddToRolesAsync(user, roles);

            TempData["Success"] = $"User '{user.Email}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, "Temp@123456");

            if (result.Succeeded)
                TempData["Success"] = "Password reset to Temp@123456. User must change on next login.";
            else
                TempData["Error"] = "Failed to reset password.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
