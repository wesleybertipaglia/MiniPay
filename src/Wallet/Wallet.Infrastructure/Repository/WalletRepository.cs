using Microsoft.EntityFrameworkCore;
using Wallet.Core.Interface;
using Wallet.Infrastructure.Data;

namespace Wallet.Infrastructure.Repository;

public class WalletRepository(AppDbContext context) : IWalletRepository
{
    public async Task<Core.Model.Wallet?> GetByUserIdAsync(Guid userId)
    {
        return await context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);
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
}
