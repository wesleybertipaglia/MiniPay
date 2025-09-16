using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Enum;
using Shared.Core.Model;
using Wallet.Core.Enum;

namespace Wallet.Core.Model;

[Index(nameof(Code), IsUnique = true, Name = "IX_Wallet_Code")]
[Index(nameof(UserId), Name = "IX_Wallet_UserId")]
public class Wallet : BaseModel
{
    [Required]
    public Guid UserId { get; private set; }

    [Range(0, double.MaxValue)]
    public decimal Balance { get; private set; }

    [Required]
    public Currency Currency { get; set; } = Currency.USD;

    [Required]
    public Country Country { get; set; } = Country.US;

    [Required]
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
        if (!HasSufficientFunds(amount))
            throw new InvalidOperationException("Insufficient funds.");

        Balance -= amount;
    }

    private bool HasSufficientFunds(decimal amount)
    {
        return Balance >= amount;
    }
}