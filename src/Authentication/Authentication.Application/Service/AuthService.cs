using System.Text.Json;
using Authentication.Core.Dto;
using Authentication.Core.Interface;
using Authentication.Core.Mapper;
using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Helper;
using Shared.Core.Interface;

namespace Authentication.Application.Service;

public class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    ICacheService cacheService, 
    IMessagePublisher messagePublisher,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<TokenDto> SignIn(SignInRequestDto signInRequestDto)
    {
        logger.LogDebug("Attempting to sign in user with email: {Email}", signInRequestDto.Email);

        var user = await userRepository.GetByEmailAsync(signInRequestDto.Email);
        if (user == null)
        {
            logger.LogWarning("Sign-in failed: user not found with email {Email}", signInRequestDto.Email);
            throw new Exception($"User with email '{signInRequestDto.Email}' not found.");
        }

        if (!user.ValidatePassword(signInRequestDto.Password))
        {
            logger.LogWarning("Sign-in failed: invalid password for email {Email}", signInRequestDto.Email);
            throw new Exception("Invalid credentials.");
        }

        logger.LogInformation("User signed in successfully: {Email}", signInRequestDto.Email);
        
        await SetUserCacheAsync(user.ToDto());

        return tokenService.GenerateJwtToken(user);
    }

    public async Task<TokenDto> SignUp(SignUpRequestDto signUpRequestDto)
    {
        logger.LogDebug("Attempting to sign up user with email: {Email}", signUpRequestDto.Email);
        
        var cachedUser = await GetUserCacheByEmailAsync(signUpRequestDto.Email);
        if (cachedUser != null)
        {
            logger.LogWarning("Sign-up failed: user already exists with email {Email}", signUpRequestDto.Email);
            throw new Exception($"User with email '{signUpRequestDto.Email}' already exists.");
        }
        
        var existingUser = await userRepository.GetByEmailAsync(signUpRequestDto.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Sign-up failed: user already exists with email {Email}", signUpRequestDto.Email);
            throw new Exception($"User with email '{signUpRequestDto.Email}' already exists.");
        }

        var newUser = signUpRequestDto.ToEntity();
        var createdUser = await userRepository.CreateAsync(newUser);
        
        await SetUserCacheAsync(createdUser.ToDto());

        logger.LogInformation("User created successfully: {Email}", signUpRequestDto.Email);

        var userDto = createdUser.ToDto();
        var messageJson = JsonSerializer.Serialize(userDto);

        await messagePublisher.PublishAsync(
            exchange: "user-exchange",
            routingKey: "user.created",
            message: messageJson
        );

        logger.LogInformation("Published 'user.created' event for user {UserId}", userDto.Id);

        return tokenService.GenerateJwtToken(createdUser);
    }

    private async Task<UserDto?> GetUserCacheByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var cacheKey = UserCacheKeys.GetUserByEmailKey(email);
        return await cacheService.GetAsync<UserDto>(cacheKey);
    }

    private async Task SetUserCacheAsync(UserDto userDto)
    {
        var expiration = TimeSpan.FromMinutes(60);
        var keyById = UserCacheKeys.GetUserByIdKey(userDto.Id);
        var keyByEmail = UserCacheKeys.GetUserByEmailKey(userDto.Email);

        await cacheService.SetAsync(keyById, userDto, expiration);
        await cacheService.SetAsync(keyByEmail, userDto, expiration);

        logger.LogDebug("Cached user {UserId} and email {Email} with expiration {Expiration}", userDto.Id, userDto.Email, expiration);
    }
}
