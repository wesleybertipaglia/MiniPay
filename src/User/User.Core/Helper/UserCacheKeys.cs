namespace User.Core.Helper;

public static class UserCacheKeys
{
    public static string GetUserByIdKey(Guid id) => $"user:id:{id}";
    public static string GetUserByCodeKey(string code) => $"user:code:{code}";
    public static string GetUserByEmailKey(string email) => $"user:email:{email}";
}