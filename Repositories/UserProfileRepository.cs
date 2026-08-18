using Microsoft.EntityFrameworkCore;
using SalaryWise.Data;
using SalaryWise.Models;

namespace SalaryWise.Repositories
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(string userId);
        Task<UserProfile> CreateAsync(UserProfile profile);
        Task UpdateAsync(UserProfile profile);
        Task<bool> ExistsAsync(string userId);
    }

    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ApplicationDbContext _db;

        public UserProfileRepository(ApplicationDbContext db) => _db = db;

        public Task<UserProfile?> GetByUserIdAsync(string userId) =>
            _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        public async Task<UserProfile> CreateAsync(UserProfile profile)
        {
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();
            return profile;
        }

        public async Task UpdateAsync(UserProfile profile)
        {
            profile.UpdatedAt = DateTime.UtcNow;
            _db.UserProfiles.Update(profile);
            await _db.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(string userId) =>
            _db.UserProfiles.AnyAsync(p => p.UserId == userId);
    }
}
