using SalaryWise.Models;

namespace SalaryWise.Services
{
    public class AffordabilityResult
    {
        public decimal DisposableIncome { get; set; }
        public decimal InvestmentAmount { get; set; }
        public decimal EmergencyFundTarget { get; set; }
        public bool IsAffordable { get; set; }
        public string? Warning { get; set; }
        public string SuggestedRange { get; set; } = string.Empty;
        public int MinSuggestedPercent { get; set; }
        public int MaxSuggestedPercent { get; set; }
    }

    public interface IAffordabilityService
    {
        AffordabilityResult Analyze(decimal salary, decimal expenses, decimal existingSavings, int investmentPercent);
    }

    /// <summary>
    /// Analyzes whether the chosen investment percentage is realistic
    /// given the user's salary and monthly expenses.
    /// </summary>
    public class AffordabilityService : IAffordabilityService
    {
        // Emergency fund = 6 months of expenses
        private const int EmergencyFundMonths = 6;
        // Investment should not exceed 85% of disposable income
        private const double MaxInvestmentRatioOfDisposable = 0.85;

        public AffordabilityResult Analyze(decimal salary, decimal expenses, decimal existingSavings, int investmentPercent)
        {
            var disposable = salary - expenses;
            var investmentAmount = salary * investmentPercent / 100m;
            var emergencyFundTarget = expenses * EmergencyFundMonths;

            // Determine min/max safe range (10–85% of disposable)
            var minAmount = disposable * 0.10m;
            var maxAmount = disposable * (decimal)MaxInvestmentRatioOfDisposable;
            int minPct = (int)Math.Max(1, Math.Floor(minAmount / salary * 100));
            int maxPct = (int)Math.Min(50, Math.Floor(maxAmount / salary * 100));

            bool isAffordable = true;
            string? warning = null;

            if (disposable <= 0)
            {
                isAffordable = false;
                warning = "⚠️ Your expenses exceed your salary. Please reduce your expenses before investing.";
            }
            else if (investmentAmount > maxAmount)
            {
                isAffordable = false;
                warning = $"⚠️ Investing ₹{investmentAmount:N0}/month ({investmentPercent}% of salary) may be too aggressive. " +
                          $"It leaves only ₹{disposable - investmentAmount:N0} for discretionary spending after expenses. " +
                          $"Consider starting with {minPct}–{maxPct}%.";
            }
            else if (investmentAmount > disposable * 0.60m && existingSavings < emergencyFundTarget)
            {
                warning = $"ℹ️ Your emergency fund (₹{existingSavings:N0}) is below the recommended ₹{emergencyFundTarget:N0}. " +
                           "Consider building that first before aggressive investing.";
            }

            return new AffordabilityResult
            {
                DisposableIncome = disposable,
                InvestmentAmount = investmentAmount,
                EmergencyFundTarget = emergencyFundTarget,
                IsAffordable = isAffordable,
                Warning = warning,
                SuggestedRange = $"{minPct}%–{maxPct}%",
                MinSuggestedPercent = minPct,
                MaxSuggestedPercent = maxPct
            };
        }
    }
}
