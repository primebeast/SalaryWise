using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalaryWise.Models;

namespace SalaryWise.Data
{
    /// <summary>
    /// Seeds the database with a demo user and sample investment scenario data.
    /// Run once on first startup.
    /// </summary>
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            try
            {
                // Apply pending EF migrations (creates all MS SQL Server tables)
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB Init Warning: {ex.Message}");
            }

            // ── Demo User ─────────────────────────────────────────
            const string demoEmail = "demo@salarywise.in";
            const string demoPassword = "Demo@123";

            if (await userManager.FindByEmailAsync(demoEmail) == null)
            {
                var demoUser = new IdentityUser
                {
                    UserName = demoEmail,
                    Email = demoEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(demoUser, demoPassword);

                if (result.Succeeded)
                {
                    // Create profile for demo user
                    var profile = new UserProfile
                    {
                        UserId = demoUser.Id,
                        FullName = "Rahul Sharma",
                        MobileNumber = "9876543210",
                        DateOfBirth = new DateTime(1992, 5, 15),
                        Occupation = "Software Engineer",
                        EmploymentType = EmploymentType.Salaried,
                        MonthlySalary = 80000,
                        MonthlyExpenses = 35000,
                        ExistingSavings = 150000,
                        City = "Bangalore",
                        RiskPreference = RiskTolerance.Medium,
                        PrimaryGoal = FinancialGoalType.WealthCreation,
                        InvestmentHorizon = InvestmentHorizon.LongTerm,
                        PreferredInvestmentPercentage = 25
                    };

                    context.UserProfiles.Add(profile);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
