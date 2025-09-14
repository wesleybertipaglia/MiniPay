using Authentication.Core.Model;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}