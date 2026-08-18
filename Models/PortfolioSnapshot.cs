using System.ComponentModel.DataAnnotations;

namespace SalaryWise.Models
{
    /// <summary>
    /// Represents a periodic snapshot of a user's portfolio allocation,
    /// used to track allocation changes over time.
    /// </summary>
    public class PortfolioSnapshot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int InvestmentPlanId { get; set; }

        public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;

        public decimal MonthlySalary { get; set; }
        public decimal MonthlyInvestment { get; set; }
        public decimal SafeAllocation { get; set; }
        public decimal MediumAllocation { get; set; }
        public int HealthScore { get; set; }

        /// <summary>JSON-serialized allocation breakdown for chart rendering</summary>
        public string? AllocationJson { get; set; }

        public virtual InvestmentPlan? InvestmentPlan { get; set; }
    }
}
