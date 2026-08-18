using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalaryWise.Models;
using SalaryWise.Repositories;
using SalaryWise.ViewModels;

namespace SalaryWise.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserProfileRepository _profileRepo;

        public ProfileController(UserManager<IdentityUser> userManager, IUserProfileRepository profileRepo)
        {
            _userManager = userManager;
            _profileRepo = profileRepo;
        }

        // GET /Profile
        public async Task<IActionResult> Index()
        {
            var user    = await _userManager.GetUserAsync(User);
            var profile = await _profileRepo.GetByUserIdAsync(user!.Id);
            return View(profile);
        }

        // GET /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user    = await _userManager.GetUserAsync(User);
            var profile = await _profileRepo.GetByUserIdAsync(user!.Id);

            if (profile == null)
                return View(new ProfileViewModel());

            return View(new ProfileViewModel
            {
                FullName                   = profile.FullName,
                MobileNumber               = profile.MobileNumber,
                DateOfBirth                = profile.DateOfBirth,
                Occupation                 = profile.Occupation,
                EmploymentType             = profile.EmploymentType,
                MonthlySalary              = profile.MonthlySalary,
                MonthlyExpenses            = profile.MonthlyExpenses,
                ExistingSavings            = profile.ExistingSavings,
                City                       = profile.City,
                RiskPreference             = profile.RiskPreference,
                PrimaryGoal                = profile.PrimaryGoal,
                InvestmentHorizon          = profile.InvestmentHorizon,
                PreferredInvestmentPercentage = profile.PreferredInvestmentPercentage
            });
        }

        // POST /Profile/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user    = await _userManager.GetUserAsync(User);
            var profile = await _profileRepo.GetByUserIdAsync(user!.Id);

            if (profile == null)
            {
                profile = new UserProfile { UserId = user.Id };
                MapToProfile(vm, profile);
                await _profileRepo.CreateAsync(profile);
            }
            else
            {
                MapToProfile(vm, profile);
                await _profileRepo.UpdateAsync(profile);
            }

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        private static void MapToProfile(ProfileViewModel vm, UserProfile p)
        {
            p.FullName                      = vm.FullName;
            p.MobileNumber                  = vm.MobileNumber;
            p.DateOfBirth                   = vm.DateOfBirth;
            p.Occupation                    = vm.Occupation;
            p.EmploymentType                = vm.EmploymentType;
            p.MonthlySalary                 = vm.MonthlySalary;
            p.MonthlyExpenses               = vm.MonthlyExpenses;
            p.ExistingSavings               = vm.ExistingSavings;
            p.City                          = vm.City;
            p.RiskPreference                = vm.RiskPreference;
            p.PrimaryGoal                   = vm.PrimaryGoal;
            p.InvestmentHorizon             = vm.InvestmentHorizon;
            p.PreferredInvestmentPercentage = vm.PreferredInvestmentPercentage;
        }
    }
}
