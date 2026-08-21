using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Implementation.SingBox.XrayModels;

public class XrayOutbound
{
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("settings")]
    public JsonElement? Settings { get; set; }

    [JsonPropertyName("streamSettings")]
    public XrayStreamSettings? StreamSettings { get; set; }
}

public class XrayVlessSettings
{
    [JsonPropertyName("vnext")]
    public List<XrayVnext> Vnext { get; set; } = [];
}

public class XrayVmessSettings
{
    [JsonPropertyName("vnext")]
    public List<XrayVnext> Vnext { get; set; } = [];
}

public class XrayVnext
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("users")]
    public List<XrayUser> Users { get; set; } = [];
}

public class XrayUser
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("encryption")]
    public string? Encryption { get; set; }

    [JsonPropertyName("flow")]
    public string? Flow { get; set; }

    [JsonPropertyName("alterId")]
    public int? AlterId { get; set; }

    [JsonPropertyName("security")]
    public string? Security { get; set; }
}

public class XrayTrojanSettings
{
    [JsonPropertyName("servers")]
    public List<XrayServerEntry> Servers { get; set; } = [];
}

public class XrayShadowsocksSettings
{
    [JsonPropertyName("servers")]
    public List<XrayServerEntry> Servers { get; set; } = [];
}

public class XrayServerEntry
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }
}

public class XrayHysteriaSettings
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }
}
