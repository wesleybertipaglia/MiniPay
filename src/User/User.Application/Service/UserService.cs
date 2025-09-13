using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;
using Shared.Core.Interface;
using User.Core.Dto;
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
            LogHelper.LogInfo(logger, "[CACHE HIT] User found in cache for ID {UserId}", [id]);
            return cachedUser;
        }

        LogHelper.LogInfo(logger, "Fetching user by ID: {UserId}", [id]);

        var user = await userRepository.GetByIdAsync(id);
        if (user == null)
        {
            LogHelper.LogWarning(logger, "User with ID {UserId} not found.", [id]);
            throw new Exception($"User with ID '{id}' not found.");
        }

        LogHelper.LogInfo(logger, "User found: {UserId}", [id]);

        var userDto = user.Map();
        await cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(5));

        return userDto;
    }

    public async Task<UserDto> GetByEmailAsync(string email)
    {
        var cacheKey = CacheKeysHelper.GetUserEmailKey(email);

        var cachedUser = await cacheService.GetAsync<UserDto>(cacheKey);
        if (cachedUser != null)
        {
            LogHelper.LogInfo(logger, "[CACHE HIT] User found in cache for Email {Email}", [email]);
            return cachedUser;
        }

        LogHelper.LogInfo(logger, "Fetching user by email: {Email}", [email]);

        var user = await userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            LogHelper.LogWarning(logger, "User with email {Email} not found.", [email]);
            throw new Exception($"User with email '{email}' not found.");
        }

        LogHelper.LogInfo(logger, "User found with email: {Email}", [email]);

        var userDto = user.Map();
        await cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(5));

        return userDto;
    }

    public async Task<UserDto> UpdateAsync(UserDto userDto)
    {
        var user = await userRepository.GetByIdAsync(userDto.Id);
        if (user == null)
        {
            LogHelper.LogWarning(logger, "User with ID {UserId} not found for update.", [userDto.Id]);
            throw new Exception($"User with ID '{userDto.Id}' not found.");
        }

        if (!string.IsNullOrEmpty(userDto.Name))
        {
            user.Name = userDto.Name;
        }
        if (!string.IsNullOrEmpty(userDto.Email))
        {
            user.Email = userDto.Email;
        }

        var updatedUser = await userRepository.UpdateAsync(user);

        var cacheKeyById = CacheKeysHelper.GetUserIdKey(updatedUser.Id);
        var cacheKeyByEmail = CacheKeysHelper.GetUserEmailKey(updatedUser.Email);

        await cacheService.RemoveAsync(cacheKeyById);
        await cacheService.RemoveAsync(cacheKeyByEmail);

        LogHelper.LogInfo(logger, "User updated: {UserId}", [updatedUser.Id]);

        return updatedUser.Map();
    }

    public Task<UserDto> ConfirmEmailAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}
