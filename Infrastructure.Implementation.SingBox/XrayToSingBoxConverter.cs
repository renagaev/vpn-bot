using System.Text.Json;
using Infrastructure.Implementation.SingBox.SingBoxModels;
using Infrastructure.Implementation.SingBox.XrayModels;

namespace Infrastructure.Implementation.SingBox;

public class XrayToSingBoxConverter : IXrayToSingBoxConverter
{
    private const string AdsRuleSetTag = "geosite-category-ads-all";
    private const string AdsRuleSetUrl =
        "https://raw.githubusercontent.com/SagerNet/sing-geosite/rule-set/geosite-category-ads-all.srs";

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public SingBoxConfig Convert(XrayConfigRoot xrayConfig)
    {
        var profile = ConvertProfile(xrayConfig);
        var outbounds = DeduplicateByIdentity(profile.Outbounds);
        DisambiguateTags(outbounds);

        var config = new SingBoxConfig
        {
            Dns = BuildDns(xrayConfig.Dns),
            Outbounds = BuildBaseOutbounds().Concat(outbounds).ToList(),
            Route = new SingBoxRoute()
        };

        if (outbounds.Count > 0)
        {
            config.Outbounds.Add(new SingBoxOutbound
            {
                Type = "selector",
                Tag = "select",
                Outbounds = outbounds.Select(o => o.Tag).ToList()
            });
            config.Route.Final = "select";
        }

        if (profile.HasBlockRule)
        {
            config.Route.RuleSet = [BuildAdsRuleSet()];
            config.Route.Rules.Add(new SingBoxRouteRule { RuleSet = [AdsRuleSetTag], Outbound = "block" });
        }

        if (profile.DirectDomains.Count > 0)
            config.Route.Rules.Add(new SingBoxRouteRule { DomainSuffix = profile.DirectDomains, Outbound = "direct" });

        if (profile.DirectIpCidrs.Count > 0)
            config.Route.Rules.Add(new SingBoxRouteRule { IpCidr = profile.DirectIpCidrs, Outbound = "direct" });

        return config;
    }

    public string ConvertJson(string xrayJson)
    {
        var xrayConfig = JsonSerializer.Deserialize<XrayConfigRoot>(xrayJson, ReadOptions)
                          ?? throw new InvalidOperationException("Не удалось разобрать конфигурацию xray");

        return JsonSerializer.Serialize(Convert(xrayConfig), WriteOptions);
    }

    public string ConvertJsonArray(string xrayJsonArray)
    {
        var xrayConfigs = JsonSerializer.Deserialize<List<XrayConfigRoot>>(xrayJsonArray, ReadOptions)
                           ?? throw new InvalidOperationException("Не удалось разобрать массив конфигураций xray");

        var profiles = xrayConfigs
            .Select(ConvertProfile)
            .ToList();

        var allOutbounds = DeduplicateByIdentity(profiles.SelectMany(p => p.Outbounds).ToList());
        DisambiguateTags(allOutbounds);

        var config = new SingBoxConfig
        {
            Dns = MergeDns(xrayConfigs),
            Outbounds = BuildBaseOutbounds().Concat(allOutbounds).ToList(),
            Route = new SingBoxRoute()
        };

        if (allOutbounds.Count > 0)
        {
            config.Outbounds.Add(new SingBoxOutbound
            {
                Type = "selector",
                Tag = "select",
                Outbounds = allOutbounds.Select(o => o.Tag).ToList()
            });
            config.Route.Final = "select";
        }

        if (profiles.Any(p => p.HasBlockRule))
        {
            config.Route.RuleSet = [BuildAdsRuleSet()];
            config.Route.Rules.Add(new SingBoxRouteRule { RuleSet = [AdsRuleSetTag], Outbound = "block" });
        }

        var directDomains = profiles.SelectMany(p => p.DirectDomains).Distinct().ToList();
        if (directDomains.Count > 0)
            config.Route.Rules.Add(new SingBoxRouteRule { DomainSuffix = directDomains, Outbound = "direct" });

        var directIps = profiles.SelectMany(p => p.DirectIpCidrs).Distinct().ToList();
        if (directIps.Count > 0)
            config.Route.Rules.Add(new SingBoxRouteRule { IpCidr = directIps, Outbound = "direct" });

        return JsonSerializer.Serialize(config, WriteOptions);
    }

    private sealed class ProfileConversionResult
    {
        public List<SingBoxOutbound> Outbounds { get; init; } = [];
        public List<string> DirectDomains { get; init; } = [];
        public List<string> DirectIpCidrs { get; init; } = [];
        public bool HasBlockRule { get; init; }
    }

    private static List<SingBoxOutbound> BuildBaseOutbounds() =>
    [
        new SingBoxOutbound { Type = "direct", Tag = "direct" },
        new SingBoxOutbound { Type = "block", Tag = "block" }
    ];

    private static SingBoxRuleSet BuildAdsRuleSet() => new()
    {
        Type = "remote",
        Tag = AdsRuleSetTag,
        Format = "binary",
        Url = AdsRuleSetUrl,
        DownloadDetour = "direct"
    };

    private static ProfileConversionResult ConvertProfile(XrayConfigRoot xrayConfig)
    {
        var outboundsByTag = new Dictionary<string, XrayOutbound>();
        var realOutboundTags = new List<string>();

        foreach (var outbound in xrayConfig.Outbounds)
        {
            if (outbound.Tag == null)
                continue;

            outboundsByTag[outbound.Tag] = outbound;

            // xhttp не поддерживается ванильным sing-box, поэтому такие outbound'ы недостижимы для клиента.
            if (outbound.StreamSettings?.Network == "xhttp")
                continue;

            if (outbound.Protocol is "freedom" or "blackhole" or "loopback")
                continue;

            realOutboundTags.Add(outbound.Tag);
        }

        var name = SanitizeTag(xrayConfig.Remarks);
        var convertedOutbounds = new List<SingBoxOutbound>();

        for (var i = 0; i < realOutboundTags.Count; i++)
        {
            var converted = ConvertOutbound(outboundsByTag[realOutboundTags[i]]);
            if (converted == null)
                continue;

            converted.Tag = realOutboundTags.Count == 1 ? name : $"{name} {i + 1}";
            convertedOutbounds.Add(converted);
        }

        var rules = xrayConfig.Routing?.Rules ?? [];

        var blockRule = rules.FirstOrDefault(r =>
            r.OutboundTag == "block" && r.Domain != null && r.Domain.Any(d => d.StartsWith("geosite:")));

        var directDomainRule = rules.FirstOrDefault(r =>
            r.OutboundTag == "direct" && r.Domain != null && r.Domain.Count > 0);

        var directIpRule = rules.FirstOrDefault(r =>
            r.OutboundTag == "direct" && r.Ip != null && r.Ip.Count > 0 && r.Port == null);

        var directDomains = (directDomainRule?.Domain ?? [])
            .Where(d => d.StartsWith("domain:"))
            .Select(d => d["domain:".Length..])
            .ToList();

        return new ProfileConversionResult
        {
            Outbounds = convertedOutbounds,
            DirectDomains = directDomains,
            DirectIpCidrs = directIpRule?.Ip ?? [],
            HasBlockRule = blockRule != null
        };
    }

    // Один и тот же физический сервер может встречаться под разными тегами (auto-select,
    // цепочки-обходы и т.д.) — оставляем только первое вхождение по идентичности сервера.
    private static List<SingBoxOutbound> DeduplicateByIdentity(List<SingBoxOutbound> outbounds)
    {
        var seen = new HashSet<(string? Type, string? Server, int Port, string? Secret)>();
        var result = new List<SingBoxOutbound>();

        foreach (var outbound in outbounds)
        {
            var key = (outbound.Type, outbound.Server, outbound.ServerPort, outbound.Uuid ?? outbound.Password);
            if (seen.Add(key))
                result.Add(outbound);
        }

        return result;
    }

    // Теги строятся из remarks и могут повторяться между профилями — делаем их уникальными,
    // чтобы каждый сервер оставался самостоятельной записью в плоском списке.
    private static void DisambiguateTags(List<SingBoxOutbound> outbounds)
    {
        var seenCounts = new Dictionary<string, int>();
        foreach (var outbound in outbounds)
        {
            if (!seenCounts.TryGetValue(outbound.Tag, out var count))
            {
                seenCounts[outbound.Tag] = 1;
                continue;
            }

            count++;
            seenCounts[outbound.Tag] = count;
            outbound.Tag = $"{outbound.Tag} ({count})";
        }
    }

    private static string SanitizeTag(string? remarks)
    {
        if (string.IsNullOrWhiteSpace(remarks))
            return $"profile-{Guid.NewGuid():N}";

        return remarks.Trim();
    }

    private static SingBoxOutbound? ConvertOutbound(XrayOutbound outbound)
    {
        var stream = outbound.StreamSettings;
        var tag = outbound.Tag!;

        switch (outbound.Protocol)
        {
            case "vless":
            {
                var settings = outbound.Settings?.Deserialize<XrayVlessSettings>(ReadOptions);
                var vnext = settings?.Vnext.FirstOrDefault();
                var user = vnext?.Users.FirstOrDefault();
                if (vnext == null || user == null)
                    return null;

                var result = new SingBoxOutbound
                {
                    Type = "vless",
                    Tag = tag,
                    Server = vnext.Address,
                    ServerPort = vnext.Port,
                    Uuid = user.Id,
                    Flow = string.IsNullOrEmpty(user.Flow) ? null : user.Flow
                };
                ApplyTls(result, stream, includeUtls: true);
                ApplyTransport(result, stream);
                return result;
            }
            case "vmess":
            {
                var settings = outbound.Settings?.Deserialize<XrayVmessSettings>(ReadOptions);
                var vnext = settings?.Vnext.FirstOrDefault();
                var user = vnext?.Users.FirstOrDefault();
                if (vnext == null || user == null)
                    return null;

                var result = new SingBoxOutbound
                {
                    Type = "vmess",
                    Tag = tag,
                    Server = vnext.Address,
                    ServerPort = vnext.Port,
                    Uuid = user.Id,
                    AlterId = user.AlterId,
                    Security = user.Security
                };
                ApplyTls(result, stream, includeUtls: true);
                ApplyTransport(result, stream);
                return result;
            }
            case "trojan":
            {
                var settings = outbound.Settings?.Deserialize<XrayTrojanSettings>(ReadOptions);
                var server = settings?.Servers.FirstOrDefault();
                if (server == null)
                    return null;

                var result = new SingBoxOutbound
                {
                    Type = "trojan",
                    Tag = tag,
                    Server = server.Address,
                    ServerPort = server.Port,
                    Password = server.Password
                };
                ApplyTls(result, stream, includeUtls: true);
                ApplyTransport(result, stream);
                return result;
            }
            case "shadowsocks":
            {
                var settings = outbound.Settings?.Deserialize<XrayShadowsocksSettings>(ReadOptions);
                var server = settings?.Servers.FirstOrDefault();
                if (server == null)
                    return null;

                return new SingBoxOutbound
                {
                    Type = "shadowsocks",
                    Tag = tag,
                    Server = server.Address,
                    ServerPort = server.Port,
                    Password = server.Password,
                    Method = server.Method
                };
            }
            case "hysteria":
            {
                var settings = outbound.Settings?.Deserialize<XrayHysteriaSettings>(ReadOptions);
                if (settings == null)
                    return null;

                var result = new SingBoxOutbound
                {
                    Type = "hysteria2",
                    Tag = tag,
                    Server = settings.Address,
                    ServerPort = settings.Port,
                    Password = stream?.HysteriaSettings?.Auth
                };
                ApplyTls(result, stream, includeUtls: false);

                var salamander = stream?.Finalmask?.Udp?.FirstOrDefault(e => e.Type == "salamander");
                if (salamander != null)
                {
                    result.Obfs = new SingBoxObfs
                    {
                        Type = "salamander",
                        Password = salamander.Settings?.Password
                    };
                }

                return result;
            }
            default:
                return null;
        }
    }

    private static void ApplyTls(SingBoxOutbound outbound, XrayStreamSettings? stream, bool includeUtls)
    {
        if (stream?.Security is not ("tls" or "reality"))
            return;

        var tls = new SingBoxTls
        {
            Enabled = true,
            ServerName = stream.Security == "reality"
                ? stream.RealitySettings?.ServerName
                : stream.TlsSettings?.ServerName,
            Alpn = stream.TlsSettings?.Alpn
        };

        var fingerprint = stream.Security == "reality"
            ? stream.RealitySettings?.Fingerprint
            : stream.TlsSettings?.Fingerprint;

        if (includeUtls && !string.IsNullOrEmpty(fingerprint))
        {
            tls.Utls = new SingBoxUtls { Enabled = true, Fingerprint = fingerprint };
        }

        if (stream.Security == "reality" && stream.RealitySettings != null)
        {
            tls.Reality = new SingBoxReality
            {
                Enabled = true,
                PublicKey = stream.RealitySettings.PublicKey,
                ShortId = stream.RealitySettings.ShortId
            };
        }

        outbound.Tls = tls;
    }

    private static void ApplyTransport(SingBoxOutbound outbound, XrayStreamSettings? stream)
    {
        if (stream?.Network == "grpc")
        {
            outbound.Transport = new SingBoxTransport
            {
                Type = "grpc",
                ServiceName = stream.GrpcSettings?.ServiceName
            };
        }
    }

    private static SingBoxDns? BuildDns(XrayDns? dns)
    {
        if (dns == null || dns.Servers.Count == 0)
            return null;

        var result = new SingBoxDns();
        var rules = new List<SingBoxDnsRule>();
        var tagIndex = 0;

        foreach (var server in dns.Servers)
        {
            if (string.IsNullOrEmpty(server.Address) || !server.Address.StartsWith("https://"))
                continue;

            var serverTag = $"dns-{tagIndex++}";
            result.Servers.Add(new SingBoxDnsServer
            {
                Type = "https",
                Tag = serverTag,
                Server = new Uri(server.Address).Host
            });

            if (server.Domains.Count > 0)
            {
                var suffixes = server.Domains
                    .Where(d => d.StartsWith("domain:"))
                    .Select(d => d["domain:".Length..])
                    .ToList();

                if (suffixes.Count > 0)
                    rules.Add(new SingBoxDnsRule { DomainSuffix = suffixes, Server = serverTag });
            }
            else
            {
                result.Final ??= serverTag;
            }
        }

        if (rules.Count > 0)
            result.Rules = rules;

        return result.Servers.Count > 0 ? result : null;
    }

    private static SingBoxDns? MergeDns(List<XrayConfigRoot> xrayConfigs)
    {
        var seenServers = new HashSet<string>();
        var merged = new SingBoxDns();
        var rules = new List<SingBoxDnsRule>();
        var tagIndex = 0;

        foreach (var config in xrayConfigs)
        {
            if (config.Dns == null)
                continue;

            foreach (var server in config.Dns.Servers)
            {
                if (string.IsNullOrEmpty(server.Address) || !server.Address.StartsWith("https://"))
                    continue;

                var host = new Uri(server.Address).Host;
                string serverTag;
                if (!seenServers.Add(host))
                {
                    serverTag = merged.Servers.First(s => s.Server == host).Tag;
                }
                else
                {
                    serverTag = $"dns-{tagIndex++}";
                    merged.Servers.Add(new SingBoxDnsServer { Type = "https", Tag = serverTag, Server = host });
                }

                if (server.Domains.Count == 0)
                {
                    merged.Final ??= serverTag;
                    continue;
                }

                var suffixes = server.Domains
                    .Where(d => d.StartsWith("domain:"))
                    .Select(d => d["domain:".Length..])
                    .ToList();

                if (suffixes.Count > 0)
                    rules.Add(new SingBoxDnsRule { DomainSuffix = suffixes, Server = serverTag });
            }
        }

        if (merged.Servers.Count == 0)
            return null;

        if (rules.Count > 0)
        {
            merged.Rules = rules
                .GroupBy(r => r.Server)
                .Select(g => new SingBoxDnsRule
                {
                    Server = g.Key,
                    DomainSuffix = g.SelectMany(r => r.DomainSuffix ?? []).Distinct().ToList()
                })
                .ToList();
        }

        return merged;
    }
}
