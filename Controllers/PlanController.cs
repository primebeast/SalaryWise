using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalaryWise.Data;
using SalaryWise.Models;
using SalaryWise.Repositories;
using SalaryWise.Services;
using SalaryWise.ViewModels;
using System.Text.Json;

namespace SalaryWise.Controllers
{
    [Authorize]
    public class PlanController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IInvestmentPlanRepository _planRepo;
        private readonly IUserProfileRepository _profileRepo;
        private readonly IRecommendationEngine _engine;
        private readonly IProjectionService _projector;
        private readonly IAffordabilityService _affordability;
        private readonly IFinancialHealthService _health;
        private readonly ApplicationDbContext _db;

        public PlanController(UserManager<IdentityUser> userManager,
            IInvestmentPlanRepository planRepo, IUserProfileRepository profileRepo,
            IRecommendationEngine engine, IProjectionService projector,
            IAffordabilityService affordability, IFinancialHealthService health,
            ApplicationDbContext db)
        {
            _userManager   = userManager;
            _planRepo      = planRepo;
            _profileRepo   = profileRepo;
            _engine        = engine;
            _projector     = projector;
            _affordability = affordability;
            _health        = health;
            _db            = db;
        }

        // GET /Plan/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user    = await _userManager.GetUserAsync(User);
            var profile = await _profileRepo.GetByUserIdAsync(user!.Id);

            // Pre-fill from profile if available
            var vm = new PlanInputViewModel();
            if (profile != null)
            {
                vm.MonthlySalary          = profile.MonthlySalary;
                vm.MonthlyExpenses        = profile.MonthlyExpenses;
                vm.ExistingSavings        = profile.ExistingSavings;
                vm.Age                    = profile.Age > 0 ? profile.Age : 25;
                vm.EmploymentType         = profile.EmploymentType;
                vm.FinancialGoal          = profile.PrimaryGoal;
                vm.InvestmentHorizon      = profile.InvestmentHorizon;
                vm.RiskTolerance          = profile.RiskPreference;
                vm.InvestmentPercentage   = profile.PreferredInvestmentPercentage;
            }
            return View(vm);
        }

        // POST /Plan/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlanInputViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);

            // Affordability
            var afford = _affordability.Analyze(vm.MonthlySalary, vm.MonthlyExpenses,
                vm.ExistingSavings, vm.InvestmentPercentage);

            decimal investAmount = afford.InvestmentAmount;

            // Recommendation
            var rec = _engine.Generate(investAmount, vm.Age, vm.RiskTolerance,
                vm.InvestmentHorizon, vm.FinancialGoal, vm.ExistingSavings, vm.MonthlyExpenses);

            // Health score
            var health = _health.Calculate(vm.MonthlySalary, vm.MonthlyExpenses,
                vm.ExistingSavings, investAmount, vm.InvestmentHorizon, rec.Allocations.Count);

            // Projections
            var projections = _projector.Project(rec);

            // Archive any current active plan
            var existingPlans = await _planRepo.GetByUserIdAsync(user!.Id);
            foreach (var p in existingPlans.Where(p => p.Status == PlanStatus.Active))
            {
                p.Status = PlanStatus.Archived;
                await _planRepo.UpdateAsync(p);
            }

            // Build and save plan
            var plan = new InvestmentPlan
            {
                UserId                 = user.Id,
                PlanName               = vm.PlanName,
                MonthlySalary          = vm.MonthlySalary,
                MonthlyExpenses        = vm.MonthlyExpenses,
                ExistingSavings        = vm.ExistingSavings,
                Age                    = vm.Age,
                EmploymentType         = vm.EmploymentType,
                FinancialGoal          = vm.FinancialGoal,
                InvestmentHorizon      = vm.InvestmentHorizon,
                RiskTolerance          = vm.RiskTolerance,
                InvestmentPercentage   = vm.InvestmentPercentage,
                MonthlyInvestmentAmount = investAmount,
                DisposableIncome       = afford.DisposableIncome,
                EmergencyFundTarget    = afford.EmergencyFundTarget,
                HealthScore            = health.TotalScore,
                IsAffordable           = afford.IsAffordable,
                AffordabilityWarning   = afford.Warning,
                SuggestedRange         = afford.SuggestedRange,
                Status                 = PlanStatus.Active,
                Recommendations = rec.Allocations.Select(a => new InvestmentRecommendation
                {
                    InstrumentName            = a.Name,
                    InstrumentCode            = a.Code,
                    Category                  = a.Category,
                    MonthlyAllocation         = a.MonthlyAmount,
                    AllocationPercentage      = a.Percentage,
                    Reason                    = a.Reason,
                    ExpectedRole              = a.ExpectedRole,
                    Liquidity                 = a.Liquidity,
                    RiskLevel                 = a.RiskLevel,
                    SuggestedHorizon          = a.SuggestedHorizon,
                    ExpectedAnnualReturnPercent = a.ExpectedAnnualReturn
                }).ToList(),
                Projections = projections.Select(p => new ProjectionResult
                {
                    Years                  = p.Years,
                    TotalInvested          = p.TotalInvested,
                    EstimatedValue         = p.EstimatedValue,
                    InflationAdjustedValue = p.InflationAdjustedValue,
                    EstimatedGains         = p.EstimatedGains,
                    BlendedCAGR            = p.BlendedCAGR
                }).ToList()
            };

            await _planRepo.CreateAsync(plan);

            // Save portfolio snapshot
            _db.PortfolioSnapshots.Add(new PortfolioSnapshot
            {
                UserId            = user.Id,
                InvestmentPlanId  = plan.Id,
                MonthlySalary     = vm.MonthlySalary,
                MonthlyInvestment = investAmount,
                SafeAllocation    = rec.SafeTotal,
                MediumAllocation  = rec.MediumTotal,
                HealthScore       = health.TotalScore,
                AllocationJson    = JsonSerializer.Serialize(rec.Allocations.Select(a => new { a.Name, a.MonthlyAmount }))
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Investment plan created successfully!";
            return RedirectToAction(nameof(Details), new { id = plan.Id });
        }

        // GET /Plan/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var plan = await _planRepo.GetByIdAsync(id, user!.Id);
            if (plan == null) return NotFound();

            return View(BuildDetailsViewModel(plan));
        }

        // GET /Plan/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var plan = await _planRepo.GetByIdAsync(id, user!.Id);
            if (plan == null) return NotFound();

            return View(new PlanInputViewModel
            {
                PlanId                = plan.Id,
                PlanName              = plan.PlanName,
                MonthlySalary         = plan.MonthlySalary,
                MonthlyExpenses       = plan.MonthlyExpenses,
                ExistingSavings       = plan.ExistingSavings,
                Age                   = plan.Age,
                EmploymentType        = plan.EmploymentType,
                FinancialGoal         = plan.FinancialGoal,
                InvestmentHorizon     = plan.InvestmentHorizon,
                RiskTolerance         = plan.RiskTolerance,
                InvestmentPercentage  = plan.InvestmentPercentage
            });
        }

        // POST /Plan/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlanInputViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            var plan = await _planRepo.GetByIdAsync(id, user!.Id);
            if (plan == null) return NotFound();

            var afford = _affordability.Analyze(vm.MonthlySalary, vm.MonthlyExpenses,
                vm.ExistingSavings, vm.InvestmentPercentage);
            decimal investAmount = afford.InvestmentAmount;

            var rec = _engine.Generate(investAmount, vm.Age, vm.RiskTolerance,
                vm.InvestmentHorizon, vm.FinancialGoal, vm.ExistingSavings, vm.MonthlyExpenses);
            var health = _health.Calculate(vm.MonthlySalary, vm.MonthlyExpenses,
                vm.ExistingSavings, investAmount, vm.InvestmentHorizon, rec.Allocations.Count);
            var projections = _projector.Project(rec);

            // Remove old recommendations/projections
            _db.InvestmentRecommendations.RemoveRange(plan.Recommendations);
            _db.ProjectionResults.RemoveRange(plan.Projections);

            plan.PlanName               = vm.PlanName;
            plan.MonthlySalary          = vm.MonthlySalary;
            plan.MonthlyExpenses        = vm.MonthlyExpenses;
            plan.ExistingSavings        = vm.ExistingSavings;
            plan.Age                    = vm.Age;
            plan.EmploymentType         = vm.EmploymentType;
            plan.FinancialGoal          = vm.FinancialGoal;
            plan.InvestmentHorizon      = vm.InvestmentHorizon;
            plan.RiskTolerance          = vm.RiskTolerance;
            plan.InvestmentPercentage   = vm.InvestmentPercentage;
            plan.MonthlyInvestmentAmount = investAmount;
            plan.DisposableIncome       = afford.DisposableIncome;
            plan.EmergencyFundTarget    = afford.EmergencyFundTarget;
            plan.HealthScore            = health.TotalScore;
            plan.IsAffordable           = afford.IsAffordable;
            plan.AffordabilityWarning   = afford.Warning;
            plan.SuggestedRange         = afford.SuggestedRange;

            plan.Recommendations = rec.Allocations.Select(a => new InvestmentRecommendation
            {
                InstrumentName             = a.Name,
                InstrumentCode             = a.Code,
                Category                   = a.Category,
                MonthlyAllocation          = a.MonthlyAmount,
                AllocationPercentage       = a.Percentage,
                Reason                     = a.Reason,
                ExpectedRole               = a.ExpectedRole,
                Liquidity                  = a.Liquidity,
                RiskLevel                  = a.RiskLevel,
                SuggestedHorizon           = a.SuggestedHorizon,
                ExpectedAnnualReturnPercent = a.ExpectedAnnualReturn
            }).ToList();

            plan.Projections = projections.Select(p => new ProjectionResult
            {
                Years                  = p.Years,
                TotalInvested          = p.TotalInvested,
                EstimatedValue         = p.EstimatedValue,
                InflationAdjustedValue = p.InflationAdjustedValue,
                EstimatedGains         = p.EstimatedGains,
                BlendedCAGR            = p.BlendedCAGR
            }).ToList();

            await _planRepo.UpdateAsync(plan);

            TempData["Success"] = "Plan updated successfully!";
            return RedirectToAction(nameof(Details), new { id = plan.Id });
        }

        // POST /Plan/Delete/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            await _planRepo.DeleteAsync(id, user!.Id);
            TempData["Success"] = "Plan deleted.";
            return RedirectToAction(nameof(History));
        }

        // POST /Plan/SetActive/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            await _planRepo.SetActiveAsync(id, user!.Id);
            TempData["Success"] = "Plan set as active.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET /Plan/History
        public async Task<IActionResult> History()
        {
            var user  = await _userManager.GetUserAsync(User);
            var plans = await _planRepo.GetByUserIdAsync(user!.Id);
            return View(plans);
        }

        // GET /Plan/Compare
        public async Task<IActionResult> Compare(int? planAId, int? planBId)
        {
            var user  = await _userManager.GetUserAsync(User);
            var all   = await _planRepo.GetByUserIdAsync(user!.Id);

            var vm = new ComparePlanViewModel
            {
                AllPlans      = all,
                SelectedPlanAId = planAId,
                SelectedPlanBId = planBId
            };

            if (planAId.HasValue)
            {
                var a = await _planRepo.GetByIdAsync(planAId.Value, user.Id);
                if (a != null) vm.PlanA = BuildDetailsViewModel(a);
            }
            if (planBId.HasValue)
            {
                var b = await _planRepo.GetByIdAsync(planBId.Value, user.Id);
                if (b != null) vm.PlanB = BuildDetailsViewModel(b);
            }

            return View(vm);
        }

        // GET /Plan/Simulator/{id}
        public async Task<IActionResult> Simulator(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var plan = await _planRepo.GetByIdAsync(id, user!.Id);
            if (plan == null) return NotFound();
            return View(BuildDetailsViewModel(plan));
        }

        // POST (AJAX) /Plan/Simulate
        [HttpPost]
        public IActionResult Simulate([FromBody] SimulateRequest req)
        {
            var afford = _affordability.Analyze(req.Salary, req.Expenses, req.ExistingSavings, req.Percent);
            var risk   = Enum.Parse<Models.RiskTolerance>(req.Risk);
            var horizon = Enum.Parse<Models.InvestmentHorizon>(req.Horizon);
            var goal   = Enum.Parse<Models.FinancialGoalType>(req.Goal);

            var rec = _engine.Generate(afford.InvestmentAmount, req.Age, risk, horizon, goal,
                req.ExistingSavings, req.Expenses);
            var projections = _projector.Project(rec);
            var health = _health.Calculate(req.Salary, req.Expenses, req.ExistingSavings,
                afford.InvestmentAmount, horizon, rec.Allocations.Count);
            var growth = _projector.GetYearByYearGrowth(rec, 20);

            return Json(new
            {
                investmentAmount = afford.InvestmentAmount,
                isAffordable     = afford.IsAffordable,
                warning          = afford.Warning,
                suggestedRange   = afford.SuggestedRange,
                healthScore      = health.TotalScore,
                grade            = health.Grade,
                safeTotal        = rec.SafeTotal,
                mediumTotal      = rec.MediumTotal,
                allocations      = rec.Allocations.Select(a => new
                {
                    a.Name, a.Code, category = a.Category.ToString(),
                    amount = a.MonthlyAmount, pct = a.Percentage
                }),
                projections = projections.Select(p => new
                {
                    p.Years, p.TotalInvested, p.EstimatedValue,
                    p.InflationAdjustedValue, p.EstimatedGains, p.BlendedCAGR
                }),
                yearlyGrowth = growth
            });
        }

        // ── Helper ──────────────────────────────────────────────────
        private PlanDetailsViewModel BuildDetailsViewModel(InvestmentPlan plan)
        {
            var afford = _affordability.Analyze(plan.MonthlySalary, plan.MonthlyExpenses,
                plan.ExistingSavings, plan.InvestmentPercentage);

            var rec = _engine.Generate(plan.MonthlyInvestmentAmount, plan.Age, plan.RiskTolerance,
                plan.InvestmentHorizon, plan.FinancialGoal, plan.ExistingSavings, plan.MonthlyExpenses);

            var projections = _projector.Project(rec);

            var health = _health.Calculate(plan.MonthlySalary, plan.MonthlyExpenses,
                plan.ExistingSavings, plan.MonthlyInvestmentAmount,
                plan.InvestmentHorizon, rec.Allocations.Count);

            var yearlyGrowth   = _projector.GetYearByYearGrowth(rec, 20);
            var yearlyInvested = Enumerable.Range(1, 20)
                .Select(y => plan.MonthlyInvestmentAmount * 12 * y).ToList();

            var colors = new[] {
                "#6366f1","#8b5cf6","#ec4899","#f59e0b","#10b981",
                "#3b82f6","#ef4444","#14b8a6","#f97316","#84cc16",
                "#06b6d4","#a855f7","#e11d48","#16a34a","#d97706"
            };

            return new PlanDetailsViewModel
            {
                Plan                 = plan,
                Recommendation       = rec,
                Projections          = projections,
                HealthScore          = health,
                Affordability        = afford,
                SimulatorPercent     = plan.InvestmentPercentage,
                SimulatorRisk        = plan.RiskTolerance.ToString(),
                AllocationLabelsJson = JsonSerializer.Serialize(rec.Allocations.Select(a => a.Name)),
                AllocationAmountsJson = JsonSerializer.Serialize(rec.Allocations.Select(a => a.MonthlyAmount)),
                AllocationColorsJson  = JsonSerializer.Serialize(colors.Take(rec.Allocations.Count)),
                GrowthLabelsJson     = JsonSerializer.Serialize(Enumerable.Range(1, 20).Select(y => $"Yr {y}")),
                GrowthValuesJson     = JsonSerializer.Serialize(yearlyGrowth),
                InvestedValuesJson   = JsonSerializer.Serialize(yearlyInvested)
            };
        }
    }

    public class SimulateRequest
    {
        public decimal Salary        { get; set; }
        public decimal Expenses      { get; set; }
        public decimal ExistingSavings { get; set; }
        public int     Age           { get; set; }
        public int     Percent       { get; set; }
        public string  Risk          { get; set; } = "Medium";
        public string  Horizon       { get; set; } = "LongTerm";
        public string  Goal          { get; set; } = "WealthCreation";
    }
}
