using System.ComponentModel.DataAnnotations;

namespace SalaryWise.Models
{
    /// <summary>
    /// Represents a complete investment plan for a user.
    /// Contains the inputs, affordability analysis, and links to recommendations.
    /// </summary>
    public class InvestmentPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Display(Name = "Plan Name")]
        public string PlanName { get; set; } = string.Empty;

        // ── Inputs ────────────────────────────────────────────────
        [Required]
        [Range(1000, 10000000)]
        [Display(Name = "Monthly Salary (₹)")]
        public decimal MonthlySalary { get; set; }

        [Required]
        [Range(0, 10000000)]
        [Display(Name = "Monthly Expenses (₹)")]
        public decimal MonthlyExpenses { get; set; }

        [Range(0, 100000000)]
        [Display(Name = "Existing Savings (₹)")]
        public decimal ExistingSavings { get; set; }

        [Range(1, 100)]
        public int Age { get; set; }

        public EmploymentType EmploymentType { get; set; }

        public FinancialGoalType FinancialGoal { get; set; }

        public InvestmentHorizon InvestmentHorizon { get; set; }

        public RiskTolerance RiskTolerance { get; set; }

        [Range(1, 50)]
        [Display(Name = "Investment Percentage (%)")]
        public int InvestmentPercentage { get; set; }

        // ── Computed ──────────────────────────────────────────────
        [Display(Name = "Monthly Investment Amount (₹)")]
        public decimal MonthlyInvestmentAmount { get; set; }

        [Display(Name = "Monthly Disposable Income (₹)")]
        public decimal DisposableIncome { get; set; }

        [Display(Name = "Emergency Fund Target (₹)")]
        public decimal EmergencyFundTarget { get; set; }

        /// <summary>Financial health score 0–100</summary>
        public int HealthScore { get; set; }

        /// <summary>Whether investment % is affordable given expenses</summary>
        public bool IsAffordable { get; set; }

        /// <summary>Warning message if not affordable</summary>
        public string? AffordabilityWarning { get; set; }

        /// <summary>Suggested safe investment % range</summary>
        public string? SuggestedRange { get; set; }

        public PlanStatus Status { get; set; } = PlanStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<InvestmentRecommendation> Recommendations { get; set; } = new List<InvestmentRecommendation>();
        public virtual ICollection<ProjectionResult> Projections { get; set; } = new List<ProjectionResult>();
    }
}
