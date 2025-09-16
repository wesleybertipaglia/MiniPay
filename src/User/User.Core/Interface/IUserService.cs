using Shared.Core.Dto;
using User.Core.Dto;

namespace User.Core.Interface;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(UserDto userDto);
    Task<UserDto> UpdateAsync(Guid userId, UserUpdateRequestDto requestDto);
}