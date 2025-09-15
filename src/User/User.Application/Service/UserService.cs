using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Interface;
using User.Core.Helper;
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
        logger.LogDebug("Getting user by ID {UserId}", id);

        var cachedUser = await GetUserCacheByIdAsync(id);
        if (cachedUser != null)
        {
            logger.LogDebug("Cache hit for user {UserId}", id);
            return cachedUser;
        }

        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            logger.LogWarning("User not found with ID {UserId}", id);
            throw new Exception($"User not found with ID {id}");
        }

        var mappedUser = user.Map();
        await CacheUserAsync(mappedUser);

        logger.LogInformation("User retrieved from database and cached: {UserId}", id);
        return mappedUser;
    }

    public async Task<UserDto> CreateAsync(UserDto userDto)
    {
        logger.LogDebug("Creating user {UserId}", userDto.Id);

        var existing = await userRepository.GetByIdAsync(userDto.Id);
        if (existing != null)
        {
            logger.LogWarning("Attempted to create user that already exists: {UserId}", userDto.Id);
            throw new Exception($"User with ID {userDto.Id} already exists");
        }

        var user = userDto.Map();
        await userRepository.CreateAsync(user);
        await InvalidateUserCacheAsync(user);

        logger.LogInformation("User created: {UserId}", user.Id);
        return user.Map();
    }

    public async Task<UserDto> UpdateAsync(UserDto userDto)
    {
        logger.LogDebug("Updating user {UserId}", userDto.Id);

        var user = await userRepository.GetByIdAsync(userDto.Id);
        if (user is null)
        {
            logger.LogWarning("User not found for update: {UserId}", userDto.Id);
            throw new Exception($"User not found with ID {userDto.Id}");
        }

        if (!string.IsNullOrEmpty(userDto.Name)) user.Name = userDto.Name;
        if (!string.IsNullOrEmpty(userDto.Email)) user.Email = userDto.Email;

        var updatedUser = await userRepository.UpdateAsync(user);
        await InvalidateUserCacheAsync(updatedUser);

        logger.LogInformation("User updated: {UserId}", updatedUser.Id);
        return updatedUser.Map();
    }

    private async Task CacheUserAsync(UserDto userDto)
    {
        var expiration = TimeSpan.FromMinutes(5);

        await cacheService.SetAsync(UserCacheKeys.GetUserByIdKey(userDto.Id), userDto, expiration);
        await cacheService.SetAsync(UserCacheKeys.GetUserByCodeKey(userDto.Code), userDto, expiration);
        await cacheService.SetAsync(UserCacheKeys.GetUserByEmailKey(userDto.Email), userDto, expiration);

        logger.LogDebug("Cached user {UserId} with expiration {Expiration}", userDto.Id, expiration);
    }

    private async Task<UserDto?> GetUserCacheByIdAsync(Guid id)
    {
        var keyById = UserCacheKeys.GetUserByIdKey(id);
        return await cacheService.GetAsync<UserDto>(keyById);
    }

    private async Task InvalidateUserCacheAsync(Core.Model.User user)
    {
        await cacheService.RemoveAsync(UserCacheKeys.GetUserByIdKey(user.Id));
        await cacheService.RemoveAsync(UserCacheKeys.GetUserByEmailKey(user.Email));

        logger.LogDebug("Invalidated cache for user {UserId}", user.Id);
    }
}
