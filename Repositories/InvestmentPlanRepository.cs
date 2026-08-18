using Microsoft.EntityFrameworkCore;
using SalaryWise.Data;
using SalaryWise.Models;

namespace SalaryWise.Repositories
{
    public interface IInvestmentPlanRepository
    {
        Task<List<InvestmentPlan>> GetByUserIdAsync(string userId);
        Task<InvestmentPlan?> GetByIdAsync(int id, string userId);
        Task<InvestmentPlan?> GetActiveAsync(string userId);
        Task<InvestmentPlan> CreateAsync(InvestmentPlan plan);
        Task UpdateAsync(InvestmentPlan plan);
        Task DeleteAsync(int id, string userId);
        Task SetActiveAsync(int id, string userId);
    }

    public class InvestmentPlanRepository : IInvestmentPlanRepository
    {
        private readonly ApplicationDbContext _db;

        public InvestmentPlanRepository(ApplicationDbContext db) => _db = db;

        public Task<List<InvestmentPlan>> GetByUserIdAsync(string userId) =>
            _db.InvestmentPlans
               .Include(p => p.Recommendations)
               .Include(p => p.Projections)
               .Where(p => p.UserId == userId)
               .OrderByDescending(p => p.CreatedAt)
               .ToListAsync();

        public Task<InvestmentPlan?> GetByIdAsync(int id, string userId) =>
            _db.InvestmentPlans
               .Include(p => p.Recommendations)
               .Include(p => p.Projections)
               .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        public Task<InvestmentPlan?> GetActiveAsync(string userId) =>
            _db.InvestmentPlans
               .Include(p => p.Recommendations)
               .Include(p => p.Projections)
               .FirstOrDefaultAsync(p => p.UserId == userId && p.Status == PlanStatus.Active);

        public async Task<InvestmentPlan> CreateAsync(InvestmentPlan plan)
        {
            _db.InvestmentPlans.Add(plan);
            await _db.SaveChangesAsync();
            return plan;
        }

        public async Task UpdateAsync(InvestmentPlan plan)
        {
            plan.UpdatedAt = DateTime.UtcNow;
            _db.InvestmentPlans.Update(plan);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var plan = await _db.InvestmentPlans.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (plan != null)
            {
                _db.InvestmentPlans.Remove(plan);
                await _db.SaveChangesAsync();
            }
        }

        public async Task SetActiveAsync(int id, string userId)
        {
            // Archive all current active plans
            var active = await _db.InvestmentPlans
                .Where(p => p.UserId == userId && p.Status == PlanStatus.Active)
                .ToListAsync();
            active.ForEach(p => p.Status = PlanStatus.Archived);

            // Activate selected
            var target = await _db.InvestmentPlans.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (target != null) target.Status = PlanStatus.Active;

            await _db.SaveChangesAsync();
        }
    }
}
