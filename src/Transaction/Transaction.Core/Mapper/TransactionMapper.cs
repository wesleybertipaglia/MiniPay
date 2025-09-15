using Shared.Core.Dto;
using Transaction.Core.Dto;

namespace Transaction.Core.Mapper;

public static class TransactionMapper
{
    public static Model.Transaction ToEntity(this TransactionRequestDto transactionRequestDto, Guid userId)
    {
        return new Model.Transaction
        (
            userId: userId,
            originalTransactionId: transactionRequestDto.OriginalTransactionId,
            targetWalletCode: transactionRequestDto.TargetWalletCode,
            type:  transactionRequestDto.TransactionType,
            amount: transactionRequestDto.Amount,
            description: transactionRequestDto.Description
        );
    }
    
    public static TransactionDetailsDto ToDetailsDto(this Model.Transaction transaction)
    {
        return new TransactionDetailsDto
        (
            Id: transaction.Id,
            Code: transaction.Code,
            UserId: transaction.UserId,
            OriginalTransactionId: transaction.OriginalTransactionId,
            TargetWalletCode: transaction.TargetWalletCode,
            Description: transaction.Description,
            Type: transaction.Type,
            Status:  transaction.Status,
            Amount: transaction.Amount,
            CreatedAt:  transaction.CreatedAt,
            UpdatedAt:  transaction.UpdatedAt
        );
    }
    
    public static TransactionSummaryDto ToSummaryDto(this Model.Transaction transaction)
    {
        return new TransactionSummaryDto
        (
            Code: transaction.Code,
            TargetWalletCode: transaction.TargetWalletCode,
            Type: transaction.Type,
            Status:  transaction.Status,
            Amount: transaction.Amount
        );
    }
}