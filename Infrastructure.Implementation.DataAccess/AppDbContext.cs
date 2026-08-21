using Domain;
using Infrastructure.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.DataAccess;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserSubscriptionAccess> UserSubscriptionAccesses { get; set; }
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>();

        modelBuilder.Entity<UserSubscriptionAccess>()
            .HasIndex(x => new { x.UserId, x.UserAgent, x.Hwid })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}