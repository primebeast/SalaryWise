using SalaryWise.Models;
using SalaryWise.Services;

namespace SalaryWise.ViewModels
{
    /// <summary>Dashboard data passed to the main dashboard view</summary>
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public UserProfile? Profile { get; set; }

        // Active plan summary
        public InvestmentPlan? ActivePlan { get; set; }
        public RecommendationResult? Recommendation { get; set; }
        public List<YearlyProjection> Projections { get; set; } = new();
        public HealthScoreBreakdown? HealthScore { get; set; }

        // All plans for history widget
        public List<InvestmentPlan> RecentPlans { get; set; } = new();

        // Chart data (serialised for Chart.js)
        public string AllocationLabelsJson { get; set; } = "[]";
        public string AllocationAmountsJson { get; set; } = "[]";
        public string GrowthLabelsJson { get; set; } = "[]";
        public string GrowthValuesJson { get; set; } = "[]";
        public string InvestedValuesJson { get; set; } = "[]";

        // Convenience
        public bool HasActivePlan => ActivePlan != null;
        public bool HasProfile   => Profile != null;
    }
}
