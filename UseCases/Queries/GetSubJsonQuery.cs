using System.Text.Json;
using Domain;
using Infrastructure.Implementation.SingBox;
using Infrastructure.Interfaces.DataAccess;
using Infrastructure.Interfaces.HappSpoofer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace UseCases.Queries;

public record GetSubJsonQuery(string Id, string? UserAgent = null, string? Hwid = null) : IRequest<Subscription>;

public record Subscription(string Json, string Title, int UpdateInterval);

public class GetSubJsonQueryHandler(
    IDbContext context,
    IMemoryCache cache,
    IHappSpoofer spoofer,
    IXrayToSingBoxConverter converter,
    IOptions<SubscriptionsSettings> options)
    : IRequestHandler<GetSubJsonQuery, Subscription?>
{
    public async Task<Subscription?> Handle(GetSubJsonQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.SubId == request.Id, cancellationToken);
        if (user != null)
            await TrackAccessAsync(user.Id, request.UserAgent, request.Hwid, cancellationToken);

        if (user is not { IsSubscribed: true })
            return BuildDirectOnlySubscription(request.UserAgent);

        var responses = await Task.WhenAll(
            options.Value.Urls.Select(url => GetCachedSubscriptionJsonAsync(url, cancellationToken)));

        return BuildSubscription(MergeSubscriptionResponses(responses), request.UserAgent);
    }

    private async Task<string> GetCachedSubscriptionJsonAsync(string url, CancellationToken cancellationToken)
    {
        if (cache.Get(url) is string cached)
            return cached;

        var value = await spoofer.GetSubscriptionJson(url, cancellationToken);
        cache.Set(url, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.Value.CacheDuration
        });

        return value;
    }

    private static string MergeSubscriptionResponses(IEnumerable<string> rawResponses)
    {
        var merged = new List<JsonElement>();
        foreach (var raw in rawResponses)
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                merged.AddRange(doc.RootElement.EnumerateArray().Select(e => e.Clone()));
            else
                merged.Add(doc.RootElement.Clone());
        }

        return JsonSerializer.Serialize(merged);
    }

    private async Task TrackAccessAsync(long userId, string? userAgent, string? hwid,
        CancellationToken cancellationToken)
    {
        var normalizedUserAgent = userAgent ?? "";
        var normalizedHwid = hwid ?? "";

        var access = await context.UserSubscriptionAccesses.FirstOrDefaultAsync(
            x => x.UserId == userId && x.UserAgent == normalizedUserAgent && x.Hwid == normalizedHwid,
            cancellationToken);

        if (access == null)
        {
            context.UserSubscriptionAccesses.Add(new UserSubscriptionAccess
            {
                UserId = userId,
                UserAgent = normalizedUserAgent,
                Hwid = normalizedHwid,
                LastSeenAt = DateTime.UtcNow
            });
        }
        else
        {
            access.LastSeenAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private Subscription BuildSubscription(string xrayJson, string? userAgent)
    {
        var resultJson = ConvertIfNeeded(xrayJson, userAgent);
        return new Subscription(resultJson, options.Value.Title, options.Value.UpdateIntervalHours);
    }

    private Subscription BuildDirectOnlySubscription(string? userAgent)
    {
        var json = IsHiddify(userAgent) ? Constants.DirectOnlySingBoxJson : Constants.DirectOnlyXrayJson;
        return new Subscription(json, Constants.DirectName, options.Value.UpdateIntervalHours);
    }

    private string ConvertIfNeeded(string xrayJson, string? userAgent)
    {
        if (IsHiddify(userAgent))
        {
            return xrayJson.StartsWith("[") ? converter.ConvertJsonArray(xrayJson) : converter.ConvertJson(xrayJson);
        }

        return xrayJson;
    }

    private static bool IsHiddify(string? userAgent) =>
        !string.IsNullOrEmpty(userAgent) && userAgent.Contains("Hiddify", StringComparison.OrdinalIgnoreCase);
}