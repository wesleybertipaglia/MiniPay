using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Helper;
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
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

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
            throw new KeyNotFoundException($"User not found with ID {id}");
        }

        var mappedUser = user.ToDto();
        await SetUserCacheAsync(mappedUser);

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
            throw new InvalidOperationException($"User with ID {userDto.Id} already exists");
        }

        var user = userDto.ToEntity();
        await userRepository.CreateAsync(user);
        await RemoveUserCacheAsync(user);

        logger.LogInformation("User created: {UserId}", user.Id);
        return user.ToDto();
    }

    public async Task<UserDto> UpdateAsync(Guid userId, UserUpdateRequestDto requestDto)
    {
        logger.LogDebug("Updating user {UserId}", userId);

        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            logger.LogWarning("User not found for update: {UserId}", userId);
            throw new KeyNotFoundException($"User not found with ID {userId}");
        }

        if (!string.IsNullOrEmpty(requestDto.Name)) user.Name = requestDto.Name;

        var updatedUser = await userRepository.UpdateAsync(user);
        await RemoveUserCacheAsync(updatedUser);

        logger.LogInformation("User updated: {UserId}", updatedUser.Id);
        return updatedUser.ToDto();
    }

    private async Task SetUserCacheAsync(UserDto userDto)
    {
        await cacheService.SetAsync(UserCacheKeys.GetUserByIdKey(userDto.Id), userDto, CacheExpiration);
        await cacheService.SetAsync(UserCacheKeys.GetUserByEmailKey(userDto.Email), userDto, CacheExpiration);

        logger.LogDebug("Cached user {UserId} with expiration {Expiration}", userDto.Id, CacheExpiration);
    }

    private async Task<UserDto?> GetUserCacheByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return null;

        return await cacheService.GetAsync<UserDto>(UserCacheKeys.GetUserByIdKey(id));
    }

    private async Task RemoveUserCacheAsync(Core.Model.User user)
    {
        await cacheService.RemoveAsync(UserCacheKeys.GetUserByIdKey(user.Id));
        await cacheService.RemoveAsync(UserCacheKeys.GetUserByEmailKey(user.Email));

        logger.LogDebug("Invalidated cache for user {UserId}", user.Id);
    }
}
