using Shared.Core.Dto;

namespace User.Core.Interface;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> GetByEmailAsync(string email);
    Task<UserDto> CreateAsync(UserDto userDto);
    Task<UserDto> UpdateAsync(UserDto userDto);
}