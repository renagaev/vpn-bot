using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Implementation.SingBox.XrayModels;

public class XrayRouting
{
    [JsonPropertyName("rules")]
    public List<XrayRoutingRule> Rules { get; set; } = [];

    [JsonPropertyName("balancers")]
    public List<XrayBalancer> Balancers { get; set; } = [];
}

public class XrayRoutingRule
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("ruleTag")]
    public string? RuleTag { get; set; }

    [JsonPropertyName("domain")]
    public List<string>? Domain { get; set; }

    [JsonPropertyName("ip")]
    public List<string>? Ip { get; set; }

    [JsonPropertyName("port")]
    public JsonElement? Port { get; set; }

    [JsonPropertyName("network")]
    public string? Network { get; set; }

    [JsonPropertyName("inboundTag")]
    public List<string>? InboundTag { get; set; }

    [JsonPropertyName("outboundTag")]
    public string? OutboundTag { get; set; }

    [JsonPropertyName("balancerTag")]
    public string? BalancerTag { get; set; }
}

public class XrayBalancer
{
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("selector")]
    public List<string> Selector { get; set; } = [];

    [JsonPropertyName("fallbackTag")]
    public string? FallbackTag { get; set; }
}
