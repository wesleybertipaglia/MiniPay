namespace User.Core.Interface;

public interface IUserRepository
{
    Task<Model.User?> GetByIdAsync(Guid id);
    Task<Model.User?> GetByEmailAsync(string email);
    Task<Model.User> CreateAsync(Model.User user);
    Task<Model.User> UpdateAsync(Model.User user);
    Task DeleteAsync(Model.User user);
}