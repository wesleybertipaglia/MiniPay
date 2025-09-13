using Microsoft.EntityFrameworkCore;
using User.Core.Interface;
using User.Infrastructure.Data;

namespace User.Infrastructure.Repository;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<Core.Model.User?> GetByIdAsync(Guid id)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Core.Model.User?> GetByEmailAsync(string email)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Core.Model.User> CreateAsync(Core.Model.User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<Core.Model.User> UpdateAsync(Core.Model.User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(Core.Model.User user)
    {
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}
