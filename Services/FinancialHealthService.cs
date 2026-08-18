using SalaryWise.Models;

namespace SalaryWise.Services
{
    public class HealthScoreBreakdown
    {
        public int TotalScore { get; set; }
        public int SavingsRateScore { get; set; }       // max 25
        public int EmergencyFundScore { get; set; }     // max 25
        public int ExpenseRatioScore { get; set; }      // max 20
        public int DiversificationScore { get; set; }  // max 15
        public int HorizonScore { get; set; }           // max 15
        public string Grade { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Tips { get; set; } = new();
    }

    public interface IFinancialHealthService
    {
        HealthScoreBreakdown Calculate(decimal salary, decimal expenses, decimal existingSavings,
            decimal investmentAmount, InvestmentHorizon horizon, int instrumentCount);
    }

    /// <summary>
    /// Computes a 0–100 financial health score based on savings rate,
    /// emergency fund coverage, expense ratio, diversification, and horizon.
    /// </summary>
    public class FinancialHealthService : IFinancialHealthService
    {
        public HealthScoreBreakdown Calculate(decimal salary, decimal expenses, decimal existingSavings,
            decimal investmentAmount, InvestmentHorizon horizon, int instrumentCount)
        {
            int savingsScore     = ScoreSavingsRate(salary, investmentAmount);
            int emergencyScore   = ScoreEmergencyFund(existingSavings, expenses);
            int expenseScore     = ScoreExpenseRatio(salary, expenses);
            int diversification  = ScoreDiversification(instrumentCount);
            int horizonScore     = ScoreHorizon(horizon);

            int total = savingsScore + emergencyScore + expenseScore + diversification + horizonScore;
            total = Math.Clamp(total, 0, 100);

            return new HealthScoreBreakdown
            {
                TotalScore           = total,
                SavingsRateScore     = savingsScore,
                EmergencyFundScore   = emergencyScore,
                ExpenseRatioScore    = expenseScore,
                DiversificationScore = diversification,
                HorizonScore         = horizonScore,
                Grade   = GetGrade(total),
                Summary = GetSummary(total),
                Tips    = GetTips(salary, expenses, existingSavings, investmentAmount, instrumentCount)
            };
        }

        // ── Scoring Functions ──────────────────────────────────────

        private int ScoreSavingsRate(decimal salary, decimal investmentAmount)
        {
            if (salary == 0) return 0;
            double rate = (double)(investmentAmount / salary) * 100;
            return rate switch
            {
                >= 30 => 25,
                >= 20 => 22,
                >= 15 => 18,
                >= 10 => 14,
                >= 5  => 10,
                _     => 5
            };
        }

        private int ScoreEmergencyFund(decimal savings, decimal expenses)
        {
            if (expenses == 0) return 0;
            double months = (double)(savings / expenses);
            return months switch
            {
                >= 6 => 25,
                >= 4 => 20,
                >= 3 => 15,
                >= 1 => 8,
                _    => 0
            };
        }

        private int ScoreExpenseRatio(decimal salary, decimal expenses)
        {
            if (salary == 0) return 0;
            double ratio = (double)(expenses / salary) * 100;
            return ratio switch
            {
                <= 40 => 20,
                <= 50 => 17,
                <= 60 => 13,
                <= 70 => 8,
                <= 80 => 4,
                _     => 0
            };
        }

        private int ScoreDiversification(int instruments)
        {
            return instruments switch
            {
                >= 7 => 15,
                >= 5 => 12,
                >= 3 => 9,
                >= 2 => 6,
                _    => 3
            };
        }

        private int ScoreHorizon(InvestmentHorizon horizon)
        {
            return horizon switch
            {
                InvestmentHorizon.LongTerm   => 15,
                InvestmentHorizon.MediumTerm => 10,
                InvestmentHorizon.ShortTerm  => 5,
                _                            => 5
            };
        }

        private string GetGrade(int score) => score switch
        {
            >= 85 => "A+",
            >= 75 => "A",
            >= 65 => "B+",
            >= 55 => "B",
            >= 45 => "C",
            _     => "D"
        };

        private string GetSummary(int score) => score switch
        {
            >= 85 => "Excellent financial health! You're on track for a secure financial future.",
            >= 75 => "Very good financial habits. Small optimisations can push you to excellent.",
            >= 65 => "Good start. Focus on boosting your savings rate and emergency fund.",
            >= 55 => "Fair. Work on reducing expenses and increasing your investment amount.",
            >= 45 => "Needs improvement. Consider professional guidance and cut discretionary spending.",
            _     => "Critical attention needed. Start with an emergency fund and reduce debt."
        };

        private List<string> GetTips(decimal salary, decimal expenses, decimal savings,
            decimal investmentAmount, int instruments)
        {
            var tips = new List<string>();
            double expenseRatio = salary > 0 ? (double)(expenses / salary) * 100 : 0;
            double savingsRate  = salary > 0 ? (double)(investmentAmount / salary) * 100 : 0;
            double emergencyMonths = expenses > 0 ? (double)(savings / expenses) : 0;

            if (expenseRatio > 60)
                tips.Add("Your expense ratio is high. Try to keep monthly expenses below 60% of income.");
            if (savingsRate < 10)
                tips.Add("Aim for at least 10–15% savings rate. Even a 5% increase makes a big difference over time.");
            if (emergencyMonths < 3)
                tips.Add("Build an emergency fund covering at least 3 months of expenses before investing aggressively.");
            if (instruments < 4)
                tips.Add("Diversify across more instruments to reduce portfolio risk.");
            if (savingsRate >= 20 && emergencyMonths >= 6)
                tips.Add("Great discipline! Consider stepping up your SIP by 10% each year.");

            if (tips.Count == 0)
                tips.Add("Keep up the great financial habits! Review your plan annually.");

            return tips;
        }
    }
}
