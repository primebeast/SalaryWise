using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalaryWise.Models;
using SalaryWise.Repositories;
using SalaryWise.ViewModels;

namespace SalaryWise.Controllers
{
    /// <summary>Handles user registration, login, logout, forgot/reset password, change password</summary>
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUserProfileRepository _profileRepo;

        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IUserProfileRepository profileRepo)
        {
            _userManager  = userManager;
            _signInManager = signInManager;
            _profileRepo  = profileRepo;
        }

        // GET /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST /Account/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new IdentityUser { UserName = vm.Email, Email = vm.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // Create a basic profile
                var profile = new UserProfile
                {
                    UserId   = user.Id,
                    FullName = vm.FullName
                };
                await _profileRepo.CreateAsync(profile);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /Account/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? "/Dashboard");

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }

        // POST /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // POST /Account/ForgotPassword
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null)
            {
                // Don't reveal whether the email exists
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // In a real app, send this by email. Here we show it directly for demo purposes.
            var resetLink = Url.Action(nameof(ResetPassword), "Account",
                new { token, email = vm.Email }, Request.Scheme);

            TempData["ResetLink"] = resetLink;
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        // GET /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token, string email) =>
            View(new ResetPasswordViewModel { Token = token, Email = email });

        // POST /Account/ResetPassword
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null) return RedirectToAction(nameof(ResetPasswordConfirmation));

            var result = await _userManager.ResetPasswordAsync(user, vm.Token, vm.Password);
            if (result.Succeeded)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();

        // GET /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword() => View();

        // POST /Account/ChangePassword
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("Index", "Profile");
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }
    }
}
