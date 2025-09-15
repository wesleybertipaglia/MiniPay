namespace Wallet.Core.Helper;

public static class LogMessages
{
    public static string WalletNotFound(Guid id) => $"Wallet with ID '{id}' not found.";
    public static string WalletCodeNotFound(string code) => $"Wallet with code '{code}' not found.";
    public static string WalletAlreadyExists(Guid id) => $"Wallet with ID '{id}' already exists.";
    public static string WalletCreated(Guid id) => $"Wallet with ID '{id}' created.";
    public static string WalletUpdated(Guid id) => $"Wallet with ID '{id}' updated.";
    public static string WalletDeleted(Guid id) => $"Wallet with ID '{id}' deleted.";
}