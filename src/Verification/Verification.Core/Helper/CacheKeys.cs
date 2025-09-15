namespace Verification.Core.Helper;

public static class CacheKeys
{
    public static string GetUserByIdKey(Guid id) => $"user:id:{id}";
    public static string GetVerificationCodeKeyByUserIdAndType(Guid userId, string type) => $"verification:user:id:{userId}:type:{type}";
}