using Shared.Core.Dto;

namespace User.Core.Mapper;

public static class UserMapper
{
    public static UserDto ToDto(this Model.User user)
    {
        return new UserDto
        (
            Id: user.Id,
            Code: user.Code,
            Name: user.Name,
            Email: user.Email,
            EmailConfirmed: user.EmailConfirmed
        );
    }
    
    public static Model.User ToEntity(this UserDto userDto)
    {
        return new Model.User
        (
            id: userDto.Id,
            code: userDto.Code,
            name: userDto.Name,
            email: userDto.Email
        );
    }
}