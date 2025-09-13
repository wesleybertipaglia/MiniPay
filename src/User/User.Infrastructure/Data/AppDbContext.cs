using Microsoft.EntityFrameworkCore;

namespace User.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Core.Model.User> Users { get; set; }
}