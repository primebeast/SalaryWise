using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SalaryWise.Models
{
    /// <summary>
    /// Extended user profile linked to ASP.NET Core Identity user.
    /// Stores financial and personal details for investment planning.
    /// </summary>
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Foreign key linking to Identity user</summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>Calculated from DateOfBirth</summary>
        public int Age => DateOfBirth.HasValue
            ? (int)((DateTime.Today - DateOfBirth.Value).TotalDays / 365.25)
            : 0;

        [MaxLength(100)]
        public string? Occupation { get; set; }

        [Display(Name = "Employment Type")]
        public EmploymentType EmploymentType { get; set; } = EmploymentType.Salaried;

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

        [MaxLength(100)]
        public string? City { get; set; }

        [Display(Name = "Risk Preference")]
        public RiskTolerance RiskPreference { get; set; } = RiskTolerance.Safe;

        [Display(Name = "Primary Financial Goal")]
        public FinancialGoalType PrimaryGoal { get; set; } = FinancialGoalType.WealthCreation;

        [Display(Name = "Investment Horizon")]
        public InvestmentHorizon InvestmentHorizon { get; set; } = InvestmentHorizon.MediumTerm;

        [Range(1, 50)]
        [Display(Name = "Preferred Investment %")]
        public int PreferredInvestmentPercentage { get; set; } = 20;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual IdentityUser? User { get; set; }
        public virtual ICollection<InvestmentPlan> InvestmentPlans { get; set; } = new List<InvestmentPlan>();
    }
}
