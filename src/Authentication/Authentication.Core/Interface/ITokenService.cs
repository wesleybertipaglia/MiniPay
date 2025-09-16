using Authentication.Core.Dto;
using Authentication.Core.Model;

namespace Authentication.Core.Interface;

public interface ITokenService
{
    TokenDto GenerateJwtToken(User user);
}