using Domain;
using Infrastructure.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.DataAccess;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDbContext
{
    public DbSet<User> Users { get; set; }
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}