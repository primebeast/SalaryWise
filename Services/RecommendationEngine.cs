using SalaryWise.Models;

namespace SalaryWise.Services
{
    /// <summary>Describes a single investment instrument recommendation</summary>
    public class InstrumentAllocation
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public InvestmentCategory Category { get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal Percentage { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ExpectedRole { get; set; } = string.Empty;
        public string Liquidity { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string SuggestedHorizon { get; set; } = string.Empty;
        public double ExpectedAnnualReturn { get; set; }
    }

    public class RecommendationResult
    {
        public List<InstrumentAllocation> Allocations { get; set; } = new();
        public decimal TotalMonthlyInvestment { get; set; }
        public decimal SafeTotal { get; set; }
        public decimal MediumTotal { get; set; }
        public double SafePercent { get; set; }
        public double MediumPercent { get; set; }
    }

    public interface IRecommendationEngine
    {
        RecommendationResult Generate(decimal monthlyInvestment, int age, RiskTolerance risk,
            InvestmentHorizon horizon, FinancialGoalType goal, decimal existingSavings, decimal expenses);
    }

    /// <summary>
    /// Rule-based recommendation engine.
    /// Allocates the monthly investment amount across safe and medium-risk instruments
    /// based on age, risk tolerance, horizon, goal, and savings cushion.
    /// </summary>
    public class RecommendationEngine : IRecommendationEngine
    {
        // ── Expected Annual Returns (%) per instrument ────────────
        private static readonly Dictionary<string, double> Returns = new()
        {
            ["PPF"]        = 7.1,
            ["EPF"]        = 8.1,
            ["NSS"]        = 7.7,
            ["GOVTBOND"]   = 7.3,
            ["RBIBOND"]    = 7.5,
            ["LIQUIDFUND"] = 6.8,
            ["STDEBT"]     = 7.0,
            ["SAVINGS"]    = 4.0,
            ["NIFTY50"]    = 12.0,
            ["NIFTYNEXT"]  = 13.0,
            ["FLEXICAP"]   = 12.5,
            ["BAF"]        = 10.0,
            ["CORPBOND"]   = 8.5,
            ["GOLDETF"]    = 8.0,
            ["DIGITALGOLD"]= 8.0,
            ["SGB"]        = 8.5,
            ["HYBRID"]     = 10.5
        };

        public RecommendationResult Generate(decimal monthlyInvestment, int age, RiskTolerance risk,
            InvestmentHorizon horizon, FinancialGoalType goal, decimal existingSavings, decimal expenses)
        {
            // ── Step 1: Determine safe vs medium split ────────────
            double safeRatio = ComputeSafeRatio(age, risk, horizon, goal, existingSavings, expenses);
            double mediumRatio = 1.0 - safeRatio;

            decimal safeAmount = Math.Round(monthlyInvestment * (decimal)safeRatio, 0);
            decimal mediumAmount = monthlyInvestment - safeAmount;

            // ── Step 2: Allocate safe instruments ────────────────
            var safeAllocations = AllocateSafe(safeAmount, age, goal, existingSavings, expenses);

            // ── Step 3: Allocate medium instruments ───────────────
            var mediumAllocations = AllocateMedium(mediumAmount, age, horizon, goal, risk);

            var all = safeAllocations.Concat(mediumAllocations).ToList();

            // Compute allocation percentages
            foreach (var a in all)
                a.Percentage = monthlyInvestment > 0 ? Math.Round(a.MonthlyAmount / monthlyInvestment * 100, 1) : 0;

            return new RecommendationResult
            {
                Allocations = all,
                TotalMonthlyInvestment = monthlyInvestment,
                SafeTotal = safeAmount,
                MediumTotal = mediumAmount,
                SafePercent = safeRatio * 100,
                MediumPercent = mediumRatio * 100
            };
        }

        // ── Safe-to-Medium Ratio Rules ────────────────────────────
        private double ComputeSafeRatio(int age, RiskTolerance risk, InvestmentHorizon horizon,
            FinancialGoalType goal, decimal existingSavings, decimal expenses)
        {
            double ratio = 0.60; // default balanced

            // Age rule: older → more safe
            if (age < 30) ratio -= 0.10;
            else if (age >= 30 && age < 40) ratio += 0.00;
            else if (age >= 40 && age < 50) ratio += 0.10;
            else ratio += 0.20;

            // Risk rule
            if (risk == RiskTolerance.Safe) ratio += 0.20;
            else ratio -= 0.10;

            // Horizon rule
            if (horizon == InvestmentHorizon.LongTerm) ratio -= 0.10;
            else if (horizon == InvestmentHorizon.ShortTerm) ratio += 0.15;

            // Goal rule
            if (goal == FinancialGoalType.Retirement) ratio += 0.05;
            if (goal == FinancialGoalType.EmergencyFund) ratio += 0.25;
            if (goal == FinancialGoalType.WealthCreation) ratio -= 0.10;

            // Low savings → more conservative
            var emergencyTarget = expenses * 6;
            if (existingSavings < emergencyTarget * 0.5m) ratio += 0.10;

            return Math.Clamp(ratio, 0.30, 0.90);
        }

        // ── Safe Instrument Allocations ───────────────────────────
        private List<InstrumentAllocation> AllocateSafe(decimal amount, int age, FinancialGoalType goal,
            decimal existingSavings, decimal expenses)
        {
            var list = new List<(string code, double weight)>();

            // PPF is almost always included (tax-exempt, guaranteed returns)
            list.Add(("PPF", 30));

            // Government bonds for stability
            list.Add(("GOVTBOND", 20));

            // Liquid fund for quick-access buffer
            list.Add(("LIQUIDFUND", 15));

            // Emergency fund goal → more liquid & safe
            if (goal == FinancialGoalType.EmergencyFund || existingSavings < expenses * 3)
                list.Add(("SAVINGS", 20));
            else
                list.Add(("STDEBT", 15));

            // EPF / NSS for salaried older investors
            if (age >= 35)
                list.Add(("NSS", 15));
            else
                list.Add(("RBIBOND", 20));

            return DistributeAmount(amount, list, InvestmentCategory.Safe);
        }

        // ── Medium Instrument Allocations ─────────────────────────
        private List<InstrumentAllocation> AllocateMedium(decimal amount, int age, InvestmentHorizon horizon,
            FinancialGoalType goal, RiskTolerance risk)
        {
            var list = new List<(string code, double weight)>();

            // Nifty 50 index fund – core equity
            list.Add(("NIFTY50", 35));

            // Gold – inflation hedge, always include
            list.Add(("GOLDETF", 15));
            list.Add(("SGB", 10));

            if (age < 35 && horizon == InvestmentHorizon.LongTerm)
            {
                // Young long-term: add growth equity
                list.Add(("NIFTYNEXT", 15));
                list.Add(("FLEXICAP", 15));
                list.Add(("HYBRID", 10));
            }
            else if (age < 45)
            {
                list.Add(("FLEXICAP", 20));
                list.Add(("BAF", 15));
                list.Add(("CORPBOND", 5));
            }
            else
            {
                // Older: favour balanced/hybrid
                list.Add(("BAF", 20));
                list.Add(("CORPBOND", 15));
                list.Add(("HYBRID", 5));
            }

            if (goal == FinancialGoalType.HomePurchase)
            {
                // Replace some equity with more stable instruments
                list = list.Select(x => x.code == "NIFTYNEXT"
                    ? ("CORPBOND", x.weight) : x).ToList();
            }

            return DistributeAmount(amount, list, InvestmentCategory.Medium);
        }

        // ── Distribute Amount Proportionally ─────────────────────
        private List<InstrumentAllocation> DistributeAmount(decimal total,
            List<(string code, double weight)> weights, InvestmentCategory category)
        {
            if (total <= 0) return new List<InstrumentAllocation>();

            double totalWeight = weights.Sum(w => w.weight);
            var allocations = new List<InstrumentAllocation>();
            decimal assigned = 0;

            for (int i = 0; i < weights.Count; i++)
            {
                var (code, weight) = weights[i];
                decimal amt = i == weights.Count - 1
                    ? total - assigned // give remainder to last
                    : Math.Round(total * (decimal)(weight / totalWeight), 0);

                assigned += amt;
                if (amt <= 0) continue;

                allocations.Add(new InstrumentAllocation
                {
                    Code = code,
                    Name = GetName(code),
                    Category = category,
                    MonthlyAmount = amt,
                    ExpectedAnnualReturn = Returns.GetValueOrDefault(code, 8.0),
                    Reason = GetReason(code),
                    ExpectedRole = GetRole(code),
                    Liquidity = GetLiquidity(code),
                    RiskLevel = category == InvestmentCategory.Safe ? "Low" : "Medium",
                    SuggestedHorizon = GetHorizon(code)
                });
            }

            return allocations;
        }

        // ── Instrument Metadata ────────────────────────────────────
        private static string GetName(string code) => code switch
        {
            "PPF"         => "Public Provident Fund (PPF)",
            "EPF"         => "Employee Provident Fund (EPF)",
            "NSS"         => "National Savings Scheme (NSS)",
            "GOVTBOND"    => "Government Bonds",
            "RBIBOND"     => "RBI Floating Rate Bonds",
            "LIQUIDFUND"  => "Liquid Fund",
            "STDEBT"      => "Short-Term Debt Fund",
            "SAVINGS"     => "High-Yield Savings Allocation",
            "NIFTY50"     => "Nifty 50 Index Fund",
            "NIFTYNEXT"   => "Nifty Next 50 Index Fund",
            "FLEXICAP"    => "Flexi-Cap Mutual Fund",
            "BAF"         => "Balanced Advantage Fund",
            "CORPBOND"    => "Corporate Bond Fund",
            "GOLDETF"     => "Gold ETF",
            "DIGITALGOLD" => "Digital Gold",
            "SGB"         => "Sovereign Gold Bond (SGB)",
            "HYBRID"      => "Hybrid Fund",
            _             => code
        };

        private static string GetReason(string code) => code switch
        {
            "PPF"         => "Tax-free returns under Section 80C with government backing. Excellent for long-term wealth with zero risk.",
            "EPF"         => "Employer-matched contributions boost effective returns. Builds a retirement corpus tax-efficiently.",
            "NSS"         => "Government-backed savings scheme with competitive interest rates and tax benefits under 80C.",
            "GOVTBOND"    => "Capital-safe government securities providing stable, predictable returns to anchor the portfolio.",
            "RBIBOND"     => "RBI-backed floating rate bonds offering sovereign safety with returns linked to NSC rates.",
            "LIQUIDFUND"  => "Highly liquid, low-risk fund for emergency access within 1 business day. Acts as a safe parking zone.",
            "STDEBT"      => "Low-duration debt fund that offers better returns than savings with minimal interest rate risk.",
            "SAVINGS"     => "Liquid savings allocation that earns better than a standard savings account while remaining instantly accessible.",
            "NIFTY50"     => "Passively tracks India's top 50 companies. Provides broad equity market exposure at very low cost.",
            "NIFTYNEXT"   => "Captures mid-large cap companies just outside Nifty 50, offering higher growth potential.",
            "FLEXICAP"    => "Fund manager can dynamically shift between large, mid, and small caps — capturing the best opportunities.",
            "BAF"         => "Dynamically adjusts equity-debt allocation based on market valuations. Reduces downside risk.",
            "CORPBOND"    => "AAA-rated corporate bonds offering higher yields than government bonds with moderate safety.",
            "GOLDETF"     => "Exchange-traded gold units providing direct gold price exposure without storage risk.",
            "DIGITALGOLD" => "Fractional gold ownership helping diversify and hedge inflation without physical storage costs.",
            "SGB"         => "Government-backed gold bonds with 2.5% annual interest on top of gold price appreciation.",
            "HYBRID"      => "Blend of equity and debt in a single fund providing balanced growth with lower volatility.",
            _             => "Part of a diversified investment strategy."
        };

        private static string GetRole(string code) => code switch
        {
            "PPF"         => "Tax-saving + long-term wealth accumulation",
            "EPF"         => "Retirement corpus builder",
            "NSS"         => "Tax-saving safe accumulation",
            "GOVTBOND"    => "Portfolio anchor / capital preservation",
            "RBIBOND"     => "Safe income generation",
            "LIQUIDFUND"  => "Emergency buffer / short-term parking",
            "STDEBT"      => "Stable income with low volatility",
            "SAVINGS"     => "Instant-access emergency reserve",
            "NIFTY50"     => "Core equity growth engine",
            "NIFTYNEXT"   => "Growth amplifier (higher return potential)",
            "FLEXICAP"    => "Dynamic equity exposure across market caps",
            "BAF"         => "Risk-managed equity participation",
            "CORPBOND"    => "Enhanced fixed income",
            "GOLDETF"     => "Inflation hedge + diversifier",
            "DIGITALGOLD" => "Inflation hedge + portfolio diversifier",
            "SGB"         => "Gold exposure with bonus interest income",
            "HYBRID"      => "One-stop balanced growth fund",
            _             => "Portfolio component"
        };

        private static string GetLiquidity(string code) => code switch
        {
            "PPF"         => "Lock-in 15 years (partial withdrawal from year 7)",
            "EPF"         => "Lock-in till retirement (partial withdrawal allowed)",
            "NSS"         => "5-year lock-in",
            "GOVTBOND"    => "Tradeable on exchange; medium liquidity",
            "RBIBOND"     => "7-year tenure; not tradeable but premature exit allowed",
            "LIQUIDFUND"  => "T+1 redemption (next business day)",
            "STDEBT"      => "T+2 to T+3 redemption",
            "SAVINGS"     => "Instantly liquid",
            "NIFTY50"     => "T+2 redemption (MF) or intraday (ETF)",
            "NIFTYNEXT"   => "T+2 redemption",
            "FLEXICAP"    => "T+2 to T+3 redemption",
            "BAF"         => "T+2 to T+3 redemption",
            "CORPBOND"    => "T+2 to T+3 redemption",
            "GOLDETF"     => "Intraday tradeable on NSE/BSE",
            "DIGITALGOLD" => "Can be sold anytime via app",
            "SGB"         => "8-year tenure; tradeable on exchange",
            "HYBRID"      => "T+2 to T+3 redemption",
            _             => "Variable"
        };

        private static string GetHorizon(string code) => code switch
        {
            "PPF"         => "10–15+ years",
            "EPF"         => "Till retirement",
            "NSS"         => "5 years",
            "GOVTBOND"    => "3–10 years",
            "RBIBOND"     => "7 years",
            "LIQUIDFUND"  => "< 1 year",
            "STDEBT"      => "1–3 years",
            "SAVINGS"     => "Always liquid",
            "NIFTY50"     => "5–10+ years",
            "NIFTYNEXT"   => "7–10+ years",
            "FLEXICAP"    => "5–10+ years",
            "BAF"         => "3–5+ years",
            "CORPBOND"    => "2–5 years",
            "GOLDETF"     => "3–10 years",
            "DIGITALGOLD" => "1–5 years",
            "SGB"         => "8 years",
            "HYBRID"      => "3–7 years",
            _             => "3–5 years"
        };
    }
}
