namespace SalaryWise.Services
{
    public class YearlyProjection
    {
        public int Years { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal EstimatedValue { get; set; }
        public decimal InflationAdjustedValue { get; set; }
        public decimal EstimatedGains { get; set; }
        public double BlendedCAGR { get; set; }
    }

    public interface IProjectionService
    {
        List<YearlyProjection> Project(RecommendationResult recommendation);
        List<decimal> GetYearByYearGrowth(RecommendationResult recommendation, int totalYears);
    }

    /// <summary>
    /// Calculates compound growth projections for 5, 10, and 20 years
    /// using per-instrument expected annual returns and an inflation adjustment of 6%.
    /// </summary>
    public class ProjectionService : IProjectionService
    {
        private const double InflationRate = 0.06; // 6% annual inflation
        private static readonly int[] ProjectionYears = { 5, 10, 20 };

        public List<YearlyProjection> Project(RecommendationResult recommendation)
        {
            var results = new List<YearlyProjection>();

            foreach (int years in ProjectionYears)
            {
                var (value, blendedCAGR) = ComputeFutureValue(recommendation.Allocations, years);
                decimal totalInvested = recommendation.TotalMonthlyInvestment * 12 * years;
                decimal gains = value - totalInvested;

                // Inflation-adjusted: divide by (1 + inflation)^years
                decimal inflationAdjusted = value / (decimal)Math.Pow(1 + InflationRate, years);

                results.Add(new YearlyProjection
                {
                    Years = years,
                    TotalInvested = Math.Round(totalInvested, 0),
                    EstimatedValue = Math.Round(value, 0),
                    InflationAdjustedValue = Math.Round(inflationAdjusted, 0),
                    EstimatedGains = Math.Round(gains, 0),
                    BlendedCAGR = Math.Round(blendedCAGR, 2)
                });
            }

            return results;
        }

        public List<decimal> GetYearByYearGrowth(RecommendationResult recommendation, int totalYears)
        {
            var points = new List<decimal>();

            for (int y = 1; y <= totalYears; y++)
            {
                var (value, _) = ComputeFutureValue(recommendation.Allocations, y);
                points.Add(Math.Round(value, 0));
            }

            return points;
        }

        /// <summary>
        /// Computes blended future value using SIP formula per instrument:
        /// FV = PMT × [((1+r/12)^n - 1) / (r/12)] × (1+r/12)
        /// where r = annual return rate, n = months
        /// </summary>
        private (decimal value, double blendedCAGR) ComputeFutureValue(
            List<InstrumentAllocation> allocations, int years)
        {
            decimal totalValue = 0;
            decimal totalMonthly = allocations.Sum(a => a.MonthlyAmount);
            double weightedReturn = 0;

            foreach (var alloc in allocations)
            {
                if (alloc.MonthlyAmount <= 0) continue;

                double annualRate = alloc.ExpectedAnnualReturn / 100.0;
                double monthlyRate = annualRate / 12.0;
                int months = years * 12;

                double fv;
                if (monthlyRate == 0)
                    fv = (double)alloc.MonthlyAmount * months;
                else
                    fv = (double)alloc.MonthlyAmount
                         * ((Math.Pow(1 + monthlyRate, months) - 1) / monthlyRate)
                         * (1 + monthlyRate);

                totalValue += (decimal)fv;

                if (totalMonthly > 0)
                    weightedReturn += alloc.ExpectedAnnualReturn * (double)(alloc.MonthlyAmount / totalMonthly);
            }

            return (totalValue, weightedReturn);
        }
    }
}
