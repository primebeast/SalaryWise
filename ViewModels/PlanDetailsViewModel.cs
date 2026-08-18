using SalaryWise.Models;
using SalaryWise.Services;

namespace SalaryWise.ViewModels
{
    /// <summary>Detailed view of a single investment plan including all recommendations and projections</summary>
    public class PlanDetailsViewModel
    {
        public InvestmentPlan Plan { get; set; } = null!;
        public RecommendationResult Recommendation { get; set; } = null!;
        public List<YearlyProjection> Projections { get; set; } = new();
        public HealthScoreBreakdown HealthScore { get; set; } = null!;
        public AffordabilityResult Affordability { get; set; } = null!;

        // Chart data
        public string AllocationLabelsJson { get; set; } = "[]";
        public string AllocationAmountsJson { get; set; } = "[]";
        public string AllocationColorsJson  { get; set; } = "[]";
        public string GrowthLabelsJson      { get; set; } = "[]";
        public string GrowthValuesJson      { get; set; } = "[]";
        public string InvestedValuesJson    { get; set; } = "[]";

        // Simulator
        public int SimulatorPercent  { get; set; }
        public string SimulatorRisk  { get; set; } = "Medium";
    }

    /// <summary>Used to compare two saved plans side-by-side</summary>
    public class ComparePlanViewModel
    {
        public List<InvestmentPlan> AllPlans { get; set; } = new();
        public PlanDetailsViewModel? PlanA { get; set; }
        public PlanDetailsViewModel? PlanB { get; set; }
        public int? SelectedPlanAId { get; set; }
        public int? SelectedPlanBId { get; set; }
    }
}
