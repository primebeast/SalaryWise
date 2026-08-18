using System.ComponentModel.DataAnnotations;
using SalaryWise.Models;

namespace SalaryWise.ViewModels
{
    /// <summary>User profile create/edit form</summary>
    public class ProfileViewModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

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
        public RiskTolerance RiskPreference { get; set; } = RiskTolerance.Medium;

        [Display(Name = "Primary Financial Goal")]
        public FinancialGoalType PrimaryGoal { get; set; } = FinancialGoalType.WealthCreation;

        [Display(Name = "Investment Horizon")]
        public InvestmentHorizon InvestmentHorizon { get; set; } = InvestmentHorizon.MediumTerm;

        [Range(1, 50)]
        [Display(Name = "Preferred Investment %")]
        public int PreferredInvestmentPercentage { get; set; } = 20;
    }
}
