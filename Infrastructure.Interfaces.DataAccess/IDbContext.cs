using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Interfaces.DataAccess;

public interface IDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserSubscriptionAccess> UserSubscriptionAccesses { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken);
}