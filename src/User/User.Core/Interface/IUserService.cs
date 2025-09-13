using User.Core.Dto;

namespace User.Core.Interface;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> GetByEmailAsync(string email);
    Task<UserDto> UpdateAsync(UserDto user);
    Task<UserDto> ConfirmEmailAsync(Guid userId);
}