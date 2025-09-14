using Authentication.Core.Dto;

namespace Authentication.Core.Interface;

public interface IAuthService
{
    Task<TokenDto> SignIn(SignInRequestDto  signInRequestDto);
    Task<TokenDto> SignUp(SignUpRequestDto signUpRequestDto);
}