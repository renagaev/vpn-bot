using Infrastructure.Interfaces.DataAccess;
using Infrastructure.Interfaces.HappSpoofer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace UseCases.Queries;

public record GetSubJsonQuery(string Id) : IRequest<string?>;

public class GetSubJsonQueryHandler(
    IDbContext context,
    IMemoryCache cache,
    IHappSpoofer spoofer,
    IOptions<SubscriptionsSettings> options)
    : IRequestHandler<GetSubJsonQuery, string?>
{
    public async Task<string?> Handle(GetSubJsonQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users.AnyAsync(x => x.SubId == request.Id && x.IsSubscribed,
            cancellationToken: cancellationToken);
        if (!user)
            return null;

        if (cache.Get(options.Value.CacheKey) is string cached)
            return cached;

        var value = await spoofer.GetSubscriptionJson(options.Value.Url, cancellationToken);
        cache.Set(options.Value.CacheKey, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.Value.CacheDuration
        });

        return value;
    }
}