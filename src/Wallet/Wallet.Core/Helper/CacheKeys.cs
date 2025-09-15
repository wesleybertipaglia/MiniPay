namespace Wallet.Core.Helper;

public static class CacheKeys
{
    public static string GetWalletByIdKey(Guid id) => $"wallet:id:{id}";
    public static string GetWalletByCodeKey(string code) => $"wallet:code:{code}";
}