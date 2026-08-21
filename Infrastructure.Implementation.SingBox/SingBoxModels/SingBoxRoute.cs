using System.Text.Json.Serialization;

namespace Infrastructure.Implementation.SingBox.SingBoxModels;

public class SingBoxRoute
{
    [JsonPropertyName("rule_set")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SingBoxRuleSet>? RuleSet { get; set; }

    [JsonPropertyName("rules")]
    public List<SingBoxRouteRule> Rules { get; set; } = [];

    [JsonPropertyName("final")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Final { get; set; }
}

public class SingBoxRouteRule
{
    [JsonPropertyName("rule_set")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? RuleSet { get; set; }

    [JsonPropertyName("domain_suffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? DomainSuffix { get; set; }

    [JsonPropertyName("ip_cidr")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? IpCidr { get; set; }

    [JsonPropertyName("network")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Network { get; set; }

    [JsonPropertyName("outbound")]
    public string Outbound { get; set; } = "";
}

public class SingBoxRuleSet
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "remote";

    [JsonPropertyName("tag")]
    public string Tag { get; set; } = "";

    [JsonPropertyName("format")]
    public string Format { get; set; } = "binary";

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("download_detour")]
    public string DownloadDetour { get; set; } = "direct";
}
