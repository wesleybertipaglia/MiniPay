using Authentication.Core.Dto;
using Authentication.Core.Model;
using Shared.Core.Dto;

namespace Authentication.Core.Mapper;

public static class UserMapper
{
    public static User Map(this SignUpRequestDto dto)
    {
        return new User
        (
            name:  dto.Name,
            email:  dto.Email,
            password: dto.Password
        );
    }

    public static UserDto Map(this User user)
    {
        return new UserDto
        (
            user.Id,
            user.Code,
            user.Name,
            user.Email,
            false
        );
    }
}