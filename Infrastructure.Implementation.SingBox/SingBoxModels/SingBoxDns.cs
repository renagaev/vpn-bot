using System.Text.Json.Serialization;

namespace Infrastructure.Implementation.SingBox.SingBoxModels;

public class SingBoxDns
{
    [JsonPropertyName("servers")]
    public List<SingBoxDnsServer> Servers { get; set; } = [];

    [JsonPropertyName("rules")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SingBoxDnsRule>? Rules { get; set; }

    [JsonPropertyName("final")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Final { get; set; }
}

public class SingBoxDnsServer
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "https";

    [JsonPropertyName("tag")]
    public string Tag { get; set; } = "";

    [JsonPropertyName("server")]
    public string Server { get; set; } = "";
}

public class SingBoxDnsRule
{
    [JsonPropertyName("domain_suffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? DomainSuffix { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = "route";

    [JsonPropertyName("server")]
    public string Server { get; set; } = "";
}
