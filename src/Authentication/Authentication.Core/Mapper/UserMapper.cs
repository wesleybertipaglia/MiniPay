using Authentication.Core.Dto;
using Authentication.Core.Model;
using Shared.Core.Dto;

namespace Authentication.Core.Mapper;

public static class UserMapper
{
    public static User Map(this SignUpRequestDto signUpRequestDto)
    {
        return new User()
        {
            Name = signUpRequestDto.Name,
            Email = signUpRequestDto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(signUpRequestDto.Password),
        };
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