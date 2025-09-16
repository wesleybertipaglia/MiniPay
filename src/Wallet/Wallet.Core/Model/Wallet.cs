using Microsoft.EntityFrameworkCore;
using Shared.Core.Enum;
using Shared.Core.Model;
using Wallet.Core.Enum;

namespace Wallet.Core.Model;

[Index(nameof(Code), IsUnique = true)]
public class Wallet : BaseModel
{
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }
    public Currency Currency { get; set; } = Currency.USD;
    public Country Country { get; set; } = Country.US;
    public WalletType Type { get; set; } = WalletType.Personal;

    public Wallet() { }
    
    public Wallet(Guid userId)
    {
        UserId = userId;
    }

    public Wallet(Guid userId, Currency currency, Country country, WalletType type)
    {
        UserId = userId;
        Currency = currency;
        Country = country;
        Type = type;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) 
            throw new InvalidOperationException("Amount must be greater than zero.");
        
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (!HasSufficientBalance(amount))
            throw new InvalidOperationException("Insufficient balance.");
        
        Balance -= amount;
    }

    private bool HasSufficientBalance(decimal amount)
    {
        return Balance >= amount;
    }
}