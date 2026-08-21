using System.Text.Json.Serialization;

namespace Infrastructure.Implementation.SingBox.XrayModels;

public class XrayConfigRoot
{
    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    [JsonPropertyName("dns")]
    public XrayDns? Dns { get; set; }

    [JsonPropertyName("outbounds")]
    public List<XrayOutbound> Outbounds { get; set; } = [];

    [JsonPropertyName("routing")]
    public XrayRouting? Routing { get; set; }
}
