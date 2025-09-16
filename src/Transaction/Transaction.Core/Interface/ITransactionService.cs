using Shared.Core.Dto;
using Shared.Core.Enum;
using Transaction.Core.Dto;

namespace Transaction.Core.Interface;

public interface ITransactionService
{
    Task<IEnumerable<TransactionSummaryDto>> ListAsync
    (
        Guid userId,
        int page = 1,
        int size = 10,
        TransactionType? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null
    );
    Task<TransactionDto> GetByCodeAsync(string code);
    Task<TransactionDto> CreateAsync(TransactionRequestDto requestDto, Guid userId);
    Task<TransactionDto> UpdateStatusAsync(TransactionUpdatesStatusDto requestDto, Guid userId);
}