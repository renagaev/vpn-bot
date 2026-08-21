using System.Text.Json.Serialization;

namespace Infrastructure.Implementation.SingBox.SingBoxModels;

public class SingBoxConfig
{
    [JsonPropertyName("dns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SingBoxDns? Dns { get; set; }

    [JsonPropertyName("outbounds")]
    public List<SingBoxOutbound> Outbounds { get; set; } = [];

    [JsonPropertyName("route")]
    public SingBoxRoute Route { get; set; } = new();
}
