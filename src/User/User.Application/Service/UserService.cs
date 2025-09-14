using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Helpers;
using Shared.Core.Interface;
using User.Core.Interface;
using User.Core.Mapper;

namespace User.Application.Service;

public class UserService(
    IUserRepository userRepository,
    ILogger<UserService> logger,
    ICacheService cacheService)
    : IUserService
{
    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var cacheKey = CacheKeysHelper.GetUserIdKey(id);

        var cachedUser = await cacheService.GetAsync<UserDto>(cacheKey);
        if (cachedUser != null)
        {
            LogHelper.LogInfo(logger, $"[CACHE HIT] User found in cache for ID {id}");
            return cachedUser;
        }

        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            var message = $"User with ID '{id}' not found.";
            LogHelper.LogError(logger, message);
            throw new Exception(message);
        }

        await cacheService.SetAsync(cacheKey, user.Map(), TimeSpan.FromMinutes(5));
        return user.Map();
    }

    public async Task<UserDto> GetByEmailAsync(string email)
    {
        var cacheKey = CacheKeysHelper.GetUserEmailKey(email);

        var cachedUser = await cacheService.GetAsync<UserDto>(cacheKey);
        if (cachedUser != null)
        {
            LogHelper.LogInfo(logger, $"[CACHE HIT] User found in cache for Email {email}");
            return cachedUser;
        }

        var user = await userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            var message = $"User with email '{email}' not found.";
            LogHelper.LogError(logger, message);
            throw new Exception(message);
        }

        await cacheService.SetAsync(cacheKey, user.Map(), TimeSpan.FromMinutes(5));
        return user.Map();
    }

    public async Task<UserDto> CreateAsync(UserDto userDto)
    {
        var existing = await userRepository.GetByIdAsync(userDto.Id);
        if (existing != null)
        {
            LogHelper.LogError(logger, $"User with ID '{userDto.Id}' already exists.");
            throw new Exception($"User with ID '{userDto.Id}' already exists.");
        }

        var user = userDto.Map();
        await userRepository.CreateAsync(user);
        
        await InvalidateUserCache(user);

        LogHelper.LogInfo(logger, $"User updated successfully: {user.Id}");
        return user.Map();
    }

    public async Task<UserDto> UpdateAsync(UserDto userDto)
    {
        var user = await userRepository.GetByIdAsync(userDto.Id);
        if (user is null)
        {
            var message = $"User with ID '{userDto.Id}' not found.";
            LogHelper.LogError(logger, message);
            throw new Exception(message);
        }
        
        if (!string.IsNullOrEmpty(userDto.Name)) user.Name = userDto.Name;
        if (!string.IsNullOrEmpty(userDto.Email)) user.Email = userDto.Email;

        var updatedUser = await userRepository.UpdateAsync(user);
        
        await InvalidateUserCache(updatedUser);

        LogHelper.LogInfo(logger, $"User updated successfully: {updatedUser.Id}");
        return updatedUser.Map();
    }

    private async Task InvalidateUserCache(Core.Model.User user)
    {
        var keyById = CacheKeysHelper.GetUserIdKey(user.Id);
        var keyByEmail = CacheKeysHelper.GetUserEmailKey(user.Email);

        await cacheService.RemoveAsync(keyById);
        await cacheService.RemoveAsync(keyByEmail);
    }
}
