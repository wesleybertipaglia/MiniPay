using Shared.Core.Enum;

namespace Transaction.Core.Validator;

public static class TransactionValidator
{
    public static void Validate(TransactionType type, string? targetWalletCode, string? targetTransactionCode)
    {
        switch (type)
        {
            case TransactionType.DEPOSIT or TransactionType.WITHDRAW when !string.IsNullOrWhiteSpace(targetWalletCode):
                throw new ArgumentException("TargetWalletCode must be null for deposit/withdraw.");
            case TransactionType.TRANSFER or TransactionType.PAYMENT when string.IsNullOrWhiteSpace(targetWalletCode):
                throw new ArgumentException("TargetWalletCode is required for transfer/payment.");
            case TransactionType.REFUND when string.IsNullOrWhiteSpace(targetTransactionCode):
                throw new ArgumentException("TargetTransactionCode is required for refund.");
        }
    }
}
