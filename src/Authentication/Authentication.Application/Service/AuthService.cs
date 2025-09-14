using System.Text.Json;
using Authentication.Core.Dto;
using Authentication.Core.Interface;
using Authentication.Core.Mapper;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;
using Shared.Core.Interface;

namespace Authentication.Application.Service;

public class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IMessagePublisher messagePublisher,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<TokenDto> SignIn(SignInRequestDto signInRequestDto)
    {
        var user = await userRepository.GetByEmailAsync(signInRequestDto.Email);
        if (user == null)
        {
            var message = $"User with email '{signInRequestDto.Email}' not found.";
            LogHelper.LogError(logger, message);
            throw new Exception(message);
        }

        if (!user.ValidatePassword(signInRequestDto.Password))
        {
            var message = "Invalid credentials.";
            LogHelper.LogError(logger, message);
            throw new Exception(message);
        }

        var token = tokenService.GenerateJwtToken(user);
        return TokenMapper.Map(token);
    }

    public async Task<TokenDto> SignUp(SignUpRequestDto signUpRequestDto)
    {
        var existingUser = await userRepository.GetByEmailAsync(signUpRequestDto.Email);
        if (existingUser != null)
        {
            var message = $"User with email '{signUpRequestDto.Email}' already exists.";
            LogHelper.LogError(logger, message);
            throw new Exception(message);
        }

        var newUser = signUpRequestDto.Map();

        var createdUser = await userRepository.CreateAsync(newUser);
        LogHelper.LogInfo(logger, $"User created successfully with email {signUpRequestDto.Email}");
        
        var userDto = createdUser.Map();
        var messageJson = JsonSerializer.Serialize(userDto);
        
        await messagePublisher.PublishAsync(
            exchange: "user-exchange",
            routingKey: "user.created",
            message: messageJson);

        var token = tokenService.GenerateJwtToken(createdUser);
        return TokenMapper.Map(token);
    }
}
