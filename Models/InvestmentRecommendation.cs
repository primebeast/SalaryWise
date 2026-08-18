using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalaryWise.Models
{
    /// <summary>
    /// A single instrument line-item in an investment plan.
    /// E.g., PPF: ₹3,000, Gold ETF: ₹1,500
    /// </summary>
    public class InvestmentRecommendation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InvestmentPlanId { get; set; }

        // ── Instrument Details ────────────────────────────────────
        [Required, MaxLength(100)]
        [Display(Name = "Instrument Name")]
        public string InstrumentName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Display(Name = "Instrument Code")]
        public string InstrumentCode { get; set; } = string.Empty;

        public InvestmentCategory Category { get; set; }

        [Range(0, 10000000)]
        [Display(Name = "Monthly Allocation (₹)")]
        public decimal MonthlyAllocation { get; set; }

        /// <summary>Percentage of total investment amount</summary>
        public decimal AllocationPercentage { get; set; }

        // ── Metadata ──────────────────────────────────────────────
        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(200)]
        [Display(Name = "Expected Role")]
        public string? ExpectedRole { get; set; }

        [MaxLength(100)]
        public string? Liquidity { get; set; }

        [MaxLength(50)]
        [Display(Name = "Risk Level")]
        public string? RiskLevel { get; set; }

        [MaxLength(100)]
        [Display(Name = "Suggested Horizon")]
        public string? SuggestedHorizon { get; set; }

        /// <summary>Expected annual return % used for projections</summary>
        public double ExpectedAnnualReturnPercent { get; set; }

        // Navigation
        [ForeignKey("InvestmentPlanId")]
        public virtual InvestmentPlan? InvestmentPlan { get; set; }
    }
}
