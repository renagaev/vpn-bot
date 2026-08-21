using Infrastructure.Implementation.SingBox;
using Infrastructure.Interfaces.DataAccess;
using Infrastructure.Interfaces.HappSpoofer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace UseCases.Queries;

public record GetSubJsonQuery(string Id, string? UserAgent = null) : IRequest<Subscription>;

public record Subscription(string Json, string Title, int UpdateInterval);

public class GetSubJsonQueryHandler(
    IDbContext context,
    IMemoryCache cache,
    IHappSpoofer spoofer,
    IOptions<SubscriptionsSettings> options)
    : IRequestHandler<GetSubJsonQuery, Subscription?>
{
    private const string EmptyXrayJson = "[]";

    private static readonly XrayToSingBoxConverter SingBoxConverter = new();

    public async Task<Subscription?> Handle(GetSubJsonQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users.AnyAsync(x => x.SubId == request.Id && x.IsSubscribed, cancellationToken);
        if (!user)
            return BuildSubscription(EmptyXrayJson, request.UserAgent);

        if (cache.Get(options.Value.CacheKey) is string cached)
            return BuildSubscription(cached, request.UserAgent);

        var value = await spoofer.GetSubscriptionJson(options.Value.Url, cancellationToken);
        cache.Set(options.Value.CacheKey, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.Value.CacheDuration
        });

        return BuildSubscription(value, request.UserAgent);
    }

    private Subscription BuildSubscription(string xrayJson, string? userAgent)
    {
        var resultJson = ConvertIfNeeded(xrayJson, userAgent);
        return new Subscription(resultJson, options.Value.Title, options.Value.UpdateIntervalHours);
    }

    private static string ConvertIfNeeded(string xrayJson, string? userAgent)
    {
        // Hiddify использует sing-box формат
        if (!string.IsNullOrEmpty(userAgent) &&
            userAgent.Contains("Hiddify", StringComparison.OrdinalIgnoreCase))
        {
            return SingBoxConverter.ConvertJsonArray(xrayJson);
        }

        return xrayJson;
    }
}