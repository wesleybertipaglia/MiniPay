namespace Wallet.Core.Helper;

public static class WalletCacheKeys
{
    public static string GetWalletByUserIdKey(Guid userId) => $"wallet:user:id:{userId}";
}