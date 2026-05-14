using AuthService.Models;

namespace AuthService.Repositories
{
    public interface IUserRepository
    {
        Task<User?> FindByEmail(string email);
        Task<User?> FindByUserId(int userId);
        Task<bool> ExistsByEmail(string email);
        Task<List<User>> FindAllByRole(string role);
        Task<List<User>> FindBySubscriptionPlan(string plan);
        Task<List<User>> FindByIsActive(bool isActive);
        Task<List<User>> GetAllUsers();
        Task Add(User user);
        Task Update(User user);
        Task DeleteByUserId(int userId);
        Task SaveChanges();
    }
}
