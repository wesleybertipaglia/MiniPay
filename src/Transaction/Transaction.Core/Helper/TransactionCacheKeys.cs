namespace Transaction.Core.Helper;

public static class TransactionCacheKeys
{
    public static string GetTransactionByIdKey(Guid id) => $"transaction:id:{id}";
    public static string GetTransactionByCodeKey(string code) => $"transaction:code:{code}";
    public static string GetTransactionsByWalletIdKey(Guid walletId) => $"transaction:wallet:{walletId}";
    public static string GetRecentTransactionsKey(int count = 10) => $"transaction:recent:{count}";
}