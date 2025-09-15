using Shared.Core.Enum;

namespace Transaction.Core.Interface;

public interface ITransactionRepository
{
    Task<IEnumerable<Core.Model.Transaction>> ListAsync(
        Guid userId,
        int page = 1,
        int size = 10,
        TransactionType? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    Task<Model.Transaction?> GetByIdAsync(Guid id);
    Task<Model.Transaction?> GetByCodeAsync(string code);
    Task<Model.Transaction> CreateAsync(Model.Transaction user);
    Task<Model.Transaction> UpdateAsync(Model.Transaction user);
}