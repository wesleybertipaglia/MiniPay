using Microsoft.EntityFrameworkCore;
using Shared.Core.Enum;
using Shared.Core.Model;

namespace Transaction.Core.Model;

[Index(nameof(Code), IsUnique = true)]
public class Transaction : BaseModel
{
    public Guid UserId { get; private set; }
    public string? TargetTransactionCode { get; private set; }
    public string? TargetWalletCode { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }

    private Transaction() { }

    public Transaction(
        Guid userId,
        string description,
        decimal amount,
        TransactionType type,
        string? targetWalletCode = null,
        string? targetTransactionCode = null)
    {
        UserId = userId;
        Description = description;
        Amount = amount;
        Type = type;
        TargetWalletCode = targetWalletCode;
        TargetTransactionCode = targetTransactionCode;
        Status = TransactionStatus.PENDING;

        Validate();
    }

    private void Validate()
    {
        if (Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        ValidateTargetWalletCode();
        ValidateTargetTransactionCode();
    }

    private void ValidateTargetWalletCode()
    {
        switch (Type)
        {
            case TransactionType.DEPOSIT:
            case TransactionType.WITHDRAW:
                if (!string.IsNullOrWhiteSpace(TargetWalletCode))
                    throw new ArgumentException($"TargetWalletCode must be null for {Type.ToString().ToLower()} transactions.");
                break;

            case TransactionType.TRANSFER:
            case TransactionType.PAYMENT:
            case TransactionType.REFUND:
                if (string.IsNullOrWhiteSpace(TargetWalletCode))
                    throw new ArgumentException($"TargetWalletCode is required for {Type.ToString().ToLower()} transactions.");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ValidateTargetTransactionCode()
    {
        switch (Type)
        {
            case TransactionType.REFUND:
                if (string.IsNullOrEmpty(TargetTransactionCode))
                    throw new ArgumentException("TargetTransactionCode is required for refund transactions.");
                break;

            default:
                if (!string.IsNullOrEmpty(TargetTransactionCode))
                    throw new ArgumentException($"TargetTransactionCode must be null for {Type.ToString().ToLower()} transactions.");
                break;
        }
    }

    public void UpdateStatus(TransactionStatus status)
    {
        switch (status)
        {
            case TransactionStatus.COMPLETED:
                MarkAsCompleted();
                break;
            case TransactionStatus.FAILED:
                MarkAsFailed();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
    
    public void MarkAsCompleted()
    {
        Status = TransactionStatus.COMPLETED;
        Touch();
    }

    public void MarkAsFailed()
    {
        Status = TransactionStatus.FAILED;
        Touch();
    }
}
