using User.Core.Dto;

namespace User.Core.Mapper;

public static class UserMapper
{
    public static UserDto Map(this Model.User user)
    {
        return new UserDto
        (
            user.Id,
            user.Name,
            user.Email,
            user.EmailConfirmed
        );
    }
}