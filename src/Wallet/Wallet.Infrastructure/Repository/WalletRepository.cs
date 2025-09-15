using Microsoft.EntityFrameworkCore;
using Wallet.Core.Interface;
using Wallet.Infrastructure.Data;

namespace Wallet.Infrastructure.Repository;

public class WalletRepository(AppDbContext context) : IWalletRepository
{
    public async Task<Core.Model.Wallet?> GetByIdAsync(Guid id)
    {
        return await context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Core.Model.Wallet?> GetByCodeAsync(string code)
    {
        return await context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Code == code);
    }

    public async Task<Core.Model.Wallet> CreateAsync(Core.Model.Wallet wallet)
    {
        await context.Wallets.AddAsync(wallet);
        await context.SaveChangesAsync();
        return wallet;
    }

    public async Task<Core.Model.Wallet> UpdateAsync(Core.Model.Wallet wallet)
    {
        context.Wallets.Update(wallet);
        await context.SaveChangesAsync();
        return wallet;
    }

    public async Task DeleteAsync(Core.Model.Wallet wallet)
    {
        context.Wallets.Remove(wallet);
        await context.SaveChangesAsync();
    }
}
