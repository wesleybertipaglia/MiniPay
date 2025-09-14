using Shared.Core.Dto;

namespace User.Core.Mapper;

public static class UserMapper
{
    public static UserDto Map(this Model.User user)
    {
        return new UserDto
        (
            user.Id,
            user.Code,
            user.Name,
            user.Email,
            user.EmailConfirmed
        );
    }
    
    public static Model.User Map(this UserDto userDto)
    {
        return new Model.User
        (
            userDto.Id,
            userDto.Code,
            userDto.Name,
            userDto.Email
        );
    }
}