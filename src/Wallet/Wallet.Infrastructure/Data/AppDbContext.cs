using Microsoft.EntityFrameworkCore;

namespace Wallet.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Core.Model.Wallet> Wallets { get; set; }
}