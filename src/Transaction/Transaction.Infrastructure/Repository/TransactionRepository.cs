using Microsoft.EntityFrameworkCore;
using Shared.Core.Enum;
using Transaction.Core.Interface;
using Transaction.Infrastructure.Data;

namespace Transaction.Infrastructure.Repository;

public class TransactionRepository(AppDbContext context) : ITransactionRepository
{
    public async Task<IEnumerable<Core.Model.Transaction>> ListAsync(
        Guid userId,
        int page = 1,
        int size = 10,
        TransactionType? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<Core.Model.Transaction?> GetByIdAsync(Guid id)
    {
        return await context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Core.Model.Transaction?> GetByCodeAsync(string code)
    {
        return await context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == code);
    }

    public async Task<Core.Model.Transaction> CreateAsync(Core.Model.Transaction transaction)
    {
        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Core.Model.Transaction> UpdateAsync(Core.Model.Transaction transaction)
    {
        context.Transactions.Update(transaction);
        await context.SaveChangesAsync();
        return transaction;
    }
}
