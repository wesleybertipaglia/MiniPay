using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Enum;
using Shared.Core.Model;

namespace Transaction.Core.Model;

[Index(nameof(Code), IsUnique = true)]
[Index(nameof(UserId))]
[Index(nameof(TargetWalletCode))]
[Index(nameof(TargetTransactionCode))]
public class Transaction : BaseModel
{
    [Required]
    public Guid UserId { get; private set; }

    [MaxLength(12)]
    public string? TargetTransactionCode { get; private set; }

    [MaxLength(12)]
    public string? TargetWalletCode { get; private set; }

    [Required]
    [MaxLength(250)]
    public string Description { get; private set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; private set; }

    [Required]
    public TransactionType Type { get; private set; }

    [Required]
    public TransactionStatus Status { get; private set; } = TransactionStatus.PENDING;
    
    private Transaction() {}

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

    public void UpdateStatus(TransactionStatus newStatus)
    {
        switch (newStatus)
        {
            case TransactionStatus.COMPLETED:
                MarkAsCompleted();
                break;
            case TransactionStatus.FAILED:
                MarkAsFailed();
                break;
            case TransactionStatus.PENDING:
            default:
                throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, "Invalid status transition.");
        }
    }
}
