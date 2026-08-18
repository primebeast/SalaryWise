using System.ComponentModel.DataAnnotations;
using SalaryWise.Models;

namespace SalaryWise.ViewModels
{
    /// <summary>Form input for creating or editing an investment plan</summary>
    public class PlanInputViewModel
    {
        public int? PlanId { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Plan Name")]
        public string PlanName { get; set; } = $"My Plan – {DateTime.Now:MMM yyyy}";

        [Required]
        [Range(1000, 10000000, ErrorMessage = "Salary must be between ₹1,000 and ₹1,00,00,000")]
        [Display(Name = "Monthly Salary (₹)")]
        public decimal MonthlySalary { get; set; }

        [Required]
        [Range(0, 10000000)]
        [Display(Name = "Monthly Expenses (₹)")]
        public decimal MonthlyExpenses { get; set; }

        [Range(0, 100000000)]
        [Display(Name = "Existing Savings (₹)")]
        public decimal ExistingSavings { get; set; }

        [Required]
        [Range(18, 80)]
        [Display(Name = "Your Age")]
        public int Age { get; set; }

        [Display(Name = "Employment Type")]
        public EmploymentType EmploymentType { get; set; } = EmploymentType.Salaried;

        [Display(Name = "Primary Financial Goal")]
        public FinancialGoalType FinancialGoal { get; set; } = FinancialGoalType.WealthCreation;

        [Display(Name = "Investment Horizon")]
        public InvestmentHorizon InvestmentHorizon { get; set; } = InvestmentHorizon.LongTerm;

        [Display(Name = "Risk Tolerance")]
        public RiskTolerance RiskTolerance { get; set; } = RiskTolerance.Medium;

        [Required]
        [Range(1, 50)]
        [Display(Name = "Investment Percentage (%)")]
        public int InvestmentPercentage { get; set; } = 20;
    }
}
