using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.ActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive);
            ViewBag.TotalRoles = await _roleManager.Roles.CountAsync();
            ViewBag.TotalPermissions = await _db.Permissions.CountAsync();
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }
    }
}
