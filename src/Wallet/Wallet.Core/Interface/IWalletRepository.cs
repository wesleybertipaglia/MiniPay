namespace Wallet.Core.Interface;

public interface IWalletRepository
{
    Task<Model.Wallet?> GetByIdAsync(Guid id);
    Task<Model.Wallet?> GetByCodeAsync(string code);
    Task<Model.Wallet> CreateAsync(Model.Wallet wallet);
    Task<Model.Wallet> UpdateAsync(Model.Wallet wallet);
    Task DeleteAsync(Model.Wallet wallet);
}