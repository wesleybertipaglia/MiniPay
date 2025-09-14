using Authentication.Core.Model;

namespace Authentication.Core.Interface;

public interface ITokenService
{
    string GenerateJwtToken(User user);
}