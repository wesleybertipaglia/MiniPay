using Authentication.Core.Model;

namespace Authentication.Core.Interface;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
}