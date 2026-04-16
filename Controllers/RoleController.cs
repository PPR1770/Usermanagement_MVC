using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public RoleController(RoleManager<ApplicationRole> roleManager, ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _db = db;
        }

        public async Task<IActionResult> Index() =>
            View(await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync());

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Role name is required.");
                return View();
            }

            if (await _roleManager.RoleExistsAsync(name))
            {
                ModelState.AddModelError("", $"Role '{name}' already exists.");
                return View();
            }

            var result = await _roleManager.CreateAsync(new ApplicationRole
            {
                Name = name,
                Description = description
            });

            if (result.Succeeded)
            {
                TempData["Success"] = $"Role '{name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string name, string description)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            role.Name = name;
            role.Description = description;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                TempData["Success"] = "Role updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (role.Name == "Admin")
            {
                TempData["Error"] = "The Admin role cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            await _roleManager.DeleteAsync(role);
            TempData["Success"] = "Role deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManagePermissions(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var allPermissions = await _db.Permissions.OrderBy(p => p.Module).ThenBy(p => p.Action).ToListAsync();
            var rolePermissions = await _db.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            ViewBag.Role = role;
            ViewBag.AllPermissions = allPermissions;
            ViewBag.RolePermissions = rolePermissions;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePermissions(string roleId, List<int> permissionIds)
        {
            var old = _db.RolePermissions.Where(rp => rp.RoleId == roleId);
            _db.RolePermissions.RemoveRange(old);

            if (permissionIds != null)
            {
                foreach (var pid in permissionIds)
                    _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Permissions saved successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
