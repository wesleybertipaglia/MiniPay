namespace Wallet.Core.Interface;

public interface IWalletRepository
{
    Task<Model.Wallet?> GetByUserIdAsync(Guid userId);
    Task<Model.Wallet?> GetByCodeAsync(string code);
    Task<Model.Wallet> CreateAsync(Model.Wallet wallet);
    Task<Model.Wallet> UpdateAsync(Model.Wallet wallet);
}