using Microsoft.EntityFrameworkCore;

namespace Transaction.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Core.Model.Transaction> Transactions { get; set; }
}