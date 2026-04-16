using ChatApp.Models;
using ChatApp.Services;
using ChatApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
        }

        // ── Login ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", model.Email);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} locked out.", model.Email);
                ModelState.AddModelError("", "Account locked out. Please try again after 5 minutes.");
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(model);
        }

        // ── Register ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("New user registered: {Email}", model.Email);
                TempData["Success"] = "Registration successful! Welcome.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // ── Logout ────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ── Forgot Password ───────────────────────────────────────────────
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.IsActive)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = Url.Action("ResetPassword", "Account",
                    new { token, email = model.Email }, Request.Scheme)!;

                var emailBody = $@"
                    <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto'>
                        <h2 style='color:#0d6efd'>Reset Your Password</h2>
                        <p>Hi {user.FirstName},</p>
                        <p>We received a request to reset your password. Click the button below:</p>
                        <a href='{resetUrl}' style='background:#0d6efd;color:#fff;padding:12px 24px;
                           text-decoration:none;border-radius:5px;display:inline-block;margin:16px 0'>
                           Reset Password
                        </a>
                        <p style='color:#888;font-size:13px'>This link expires in 24 hours. If you didn't request this, ignore this email.</p>
                    </div>";

                await _emailService.SendEmailAsync(model.Email, "Reset Your Password", emailBody);
            }

            // Always show success (prevents email enumeration)
            TempData["Success"] = "If that email exists, a reset link has been sent.";
            return RedirectToAction("Login");
        }

        // ── Reset Password ────────────────────────────────────────────────
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["Success"] = "Password has been reset successfully.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["Success"] = "Password reset successfully. Please login with your new password.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // ── Change Password ───────────────────────────────────────────────
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        public IActionResult AccessDenied() => View();

        [HttpGet]
        public string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
