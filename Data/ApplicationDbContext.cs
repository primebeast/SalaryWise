using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalaryWise.Models;

namespace SalaryWise.Data
{
    /// <summary>
    /// Main database context for SalaryWise.
    /// Extends IdentityDbContext to include Identity tables alongside custom tables.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ── Custom Tables ─────────────────────────────────────────
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<InvestmentPlan> InvestmentPlans { get; set; }
        public DbSet<InvestmentRecommendation> InvestmentRecommendations { get; set; }
        public DbSet<ProjectionResult> ProjectionResults { get; set; }
        public DbSet<PortfolioSnapshot> PortfolioSnapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // UserProfile → IdentityUser (one-to-one)
            builder.Entity<UserProfile>()
                .HasIndex(u => u.UserId)
                .IsUnique();

            builder.Entity<UserProfile>()
                .HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // InvestmentPlan – decimal precision
            builder.Entity<InvestmentPlan>()
                .Property(p => p.MonthlySalary)
                .HasColumnType("decimal(18,2)");

            builder.Entity<InvestmentPlan>()
                .Property(p => p.MonthlyExpenses)
                .HasColumnType("decimal(18,2)");

            builder.Entity<InvestmentPlan>()
                .Property(p => p.ExistingSavings)
                .HasColumnType("decimal(18,2)");

            builder.Entity<InvestmentPlan>()
                .Property(p => p.MonthlyInvestmentAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<InvestmentPlan>()
                .Property(p => p.DisposableIncome)
                .HasColumnType("decimal(18,2)");

            builder.Entity<InvestmentPlan>()
                .Property(p => p.EmergencyFundTarget)
                .HasColumnType("decimal(18,2)");

            // InvestmentRecommendation – decimal precision
            builder.Entity<InvestmentRecommendation>()
                .Property(r => r.MonthlyAllocation)
                .HasColumnType("decimal(18,2)");

            builder.Entity<InvestmentRecommendation>()
                .Property(r => r.AllocationPercentage)
                .HasColumnType("decimal(5,2)");

            // ProjectionResult – decimal precision
            builder.Entity<ProjectionResult>()
                .Property(p => p.TotalInvested)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ProjectionResult>()
                .Property(p => p.EstimatedValue)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ProjectionResult>()
                .Property(p => p.InflationAdjustedValue)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ProjectionResult>()
                .Property(p => p.EstimatedGains)
                .HasColumnType("decimal(18,2)");

            // UserProfile – decimal precision
            builder.Entity<UserProfile>()
                .Property(u => u.MonthlySalary)
                .HasColumnType("decimal(18,2)");

            builder.Entity<UserProfile>()
                .Property(u => u.MonthlyExpenses)
                .HasColumnType("decimal(18,2)");

            builder.Entity<UserProfile>()
                .Property(u => u.ExistingSavings)
                .HasColumnType("decimal(18,2)");

            // PortfolioSnapshot – decimal precision
            builder.Entity<PortfolioSnapshot>()
                .Property(s => s.MonthlySalary)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PortfolioSnapshot>()
                .Property(s => s.MonthlyInvestment)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PortfolioSnapshot>()
                .Property(s => s.SafeAllocation)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PortfolioSnapshot>()
                .Property(s => s.MediumAllocation)
                .HasColumnType("decimal(18,2)");
        }
    }
}
