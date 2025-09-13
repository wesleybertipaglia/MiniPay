using Microsoft.EntityFrameworkCore;
using Verification.Core.Model;

namespace Verification.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<VerificationCode> VerificationCodes { get; set; }
}