using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Interfaces.DataAccess;

public interface IDbContext
{
    DbSet<User> Users { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken);
}