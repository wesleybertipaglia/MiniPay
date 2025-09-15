using Shared.Core.Dto;
using Shared.Core.Enum;
using Transaction.Core.Dto;

namespace Transaction.Core.Interface;

public interface ITransactionService
{
    Task<IEnumerable<TransactionSummaryDto>> ListAsync
    (
        Guid walletId,
        int page = 1,
        int size = 10,
        TransactionType? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null
    );
    Task<TransactionDetailsDto> GetByIdAsync(Guid id);
    Task<TransactionDetailsDto> CreateAsync(TransactionRequestDto request, Guid userId);
    Task<TransactionDetailsDto> UpdateStatusAsync(TransactionDto request, Guid userId);
}