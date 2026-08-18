using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalaryWise.Models
{
    /// <summary>
    /// Stores wealth projection data for a given plan at 5, 10, and 20-year horizons.
    /// </summary>
    public class ProjectionResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InvestmentPlanId { get; set; }

        /// <summary>Projection year (5, 10, or 20)</summary>
        public int Years { get; set; }

        /// <summary>Total amount invested over the period</summary>
        public decimal TotalInvested { get; set; }

        /// <summary>Estimated portfolio value at end of period (nominal)</summary>
        public decimal EstimatedValue { get; set; }

        /// <summary>Estimated portfolio value adjusted for 6% inflation</summary>
        public decimal InflationAdjustedValue { get; set; }

        /// <summary>Total gains (EstimatedValue - TotalInvested)</summary>
        public decimal EstimatedGains { get; set; }

        /// <summary>CAGR % for the blended portfolio</summary>
        public double BlendedCAGR { get; set; }

        [ForeignKey("InvestmentPlanId")]
        public virtual InvestmentPlan? InvestmentPlan { get; set; }
    }
}
