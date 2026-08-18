using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalaryWise.Repositories;
using SalaryWise.Services;
using SalaryWise.ViewModels;
using System.Text.Json;

namespace SalaryWise.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserProfileRepository _profileRepo;
        private readonly IInvestmentPlanRepository _planRepo;
        private readonly IRecommendationEngine _engine;
        private readonly IProjectionService _projector;
        private readonly IFinancialHealthService _health;

        public DashboardController(UserManager<IdentityUser> userManager,
            IUserProfileRepository profileRepo, IInvestmentPlanRepository planRepo,
            IRecommendationEngine engine, IProjectionService projector,
            IFinancialHealthService health)
        {
            _userManager = userManager;
            _profileRepo = profileRepo;
            _planRepo    = planRepo;
            _engine      = engine;
            _projector   = projector;
            _health      = health;
        }

        public async Task<IActionResult> Index()
        {
            var user    = await _userManager.GetUserAsync(User);
            var profile = await _profileRepo.GetByUserIdAsync(user!.Id);
            var plans   = await _planRepo.GetByUserIdAsync(user.Id);
            var active  = plans.FirstOrDefault(p => p.Status == Models.PlanStatus.Active)
                       ?? plans.FirstOrDefault();

            var vm = new DashboardViewModel
            {
                UserName    = profile?.FullName ?? user.Email ?? "User",
                Profile     = profile,
                ActivePlan  = active,
                RecentPlans = plans.Take(5).ToList()
            };

            if (active != null)
            {
                // Re-generate live recommendation from plan inputs
                var rec = _engine.Generate(
                    active.MonthlyInvestmentAmount, active.Age, active.RiskTolerance,
                    active.InvestmentHorizon, active.FinancialGoal,
                    active.ExistingSavings, active.MonthlyExpenses);

                var projections = _projector.Project(rec);
                var health = _health.Calculate(active.MonthlySalary, active.MonthlyExpenses,
                    active.ExistingSavings, active.MonthlyInvestmentAmount,
                    active.InvestmentHorizon, rec.Allocations.Count);

                // Year-by-year growth for line chart (20 years)
                var yearlyGrowth  = _projector.GetYearByYearGrowth(rec, 20);
                var yearlyInvested = Enumerable.Range(1, 20)
                    .Select(y => active.MonthlyInvestmentAmount * 12 * y).ToList();

                vm.Recommendation       = rec;
                vm.Projections          = projections;
                vm.HealthScore          = health;
                vm.AllocationLabelsJson = JsonSerializer.Serialize(rec.Allocations.Select(a => a.Name));
                vm.AllocationAmountsJson = JsonSerializer.Serialize(rec.Allocations.Select(a => a.MonthlyAmount));
                vm.GrowthLabelsJson     = JsonSerializer.Serialize(Enumerable.Range(1, 20).Select(y => $"Year {y}"));
                vm.GrowthValuesJson     = JsonSerializer.Serialize(yearlyGrowth);
                vm.InvestedValuesJson   = JsonSerializer.Serialize(yearlyInvested);
            }

            return View(vm);
        }
    }
}
