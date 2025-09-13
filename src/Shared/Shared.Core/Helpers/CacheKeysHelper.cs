namespace Shared.Core.Helpers;

public static class CacheKeysHelper
{
    public static string GetUserIdKey(Guid id) => $"user:id:{id}";
    public static string GetUserEmailKey(string email) => $"user:email:{email}";
}