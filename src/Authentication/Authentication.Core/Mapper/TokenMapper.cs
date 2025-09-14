using Authentication.Core.Dto;

namespace Authentication.Core.Mapper;

public class TokenMapper
{
    public static TokenDto Map(string token)
    {
        return new TokenDto
        (
            token, 
            DateTime.Now.AddHours(1)
        );
    }
}