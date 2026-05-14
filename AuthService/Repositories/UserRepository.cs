using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> FindByUserId(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> ExistsByEmail(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<List<User>> FindAllByRole(string role)
        {
            return await _context.Users.Where(u => u.Role == role).ToListAsync();
        }

        public async Task<List<User>> FindBySubscriptionPlan(string plan)
        {
            return await _context.Users.Where(u => u.SubscriptionPlan == plan).ToListAsync();
        }

        public async Task<List<User>> FindByIsActive(bool isActive)
        {
            return await _context.Users.Where(u => u.IsActive == isActive).ToListAsync();
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task Add(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task Update(User user)
        {
            _context.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task DeleteByUserId(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
