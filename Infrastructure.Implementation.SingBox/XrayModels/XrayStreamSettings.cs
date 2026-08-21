using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Implementation.SingBox.XrayModels;

public class XrayStreamSettings
{
    [JsonPropertyName("network")]
    public string? Network { get; set; }

    [JsonPropertyName("security")]
    public string? Security { get; set; }

    [JsonPropertyName("grpcSettings")]
    public XrayGrpcSettings? GrpcSettings { get; set; }

    [JsonPropertyName("xhttpSettings")]
    public JsonElement? XhttpSettings { get; set; }

    [JsonPropertyName("tlsSettings")]
    public XrayTlsSettings? TlsSettings { get; set; }

    [JsonPropertyName("realitySettings")]
    public XrayRealitySettings? RealitySettings { get; set; }

    [JsonPropertyName("hysteriaSettings")]
    public XrayHysteriaStreamSettings? HysteriaSettings { get; set; }

    [JsonPropertyName("finalmask")]
    public XrayFinalmask? Finalmask { get; set; }

    [JsonPropertyName("sockopt")]
    public XraySockopt? Sockopt { get; set; }
}

public class XrayGrpcSettings
{
    [JsonPropertyName("serviceName")]
    public string? ServiceName { get; set; }
}

public class XrayTlsSettings
{
    [JsonPropertyName("serverName")]
    public string? ServerName { get; set; }

    [JsonPropertyName("fingerprint")]
    public string? Fingerprint { get; set; }

    [JsonPropertyName("alpn")]
    public List<string>? Alpn { get; set; }
}

public class XrayRealitySettings
{
    [JsonPropertyName("serverName")]
    public string? ServerName { get; set; }

    [JsonPropertyName("fingerprint")]
    public string? Fingerprint { get; set; }

    [JsonPropertyName("publicKey")]
    public string? PublicKey { get; set; }

    [JsonPropertyName("shortId")]
    public string? ShortId { get; set; }
}

public class XrayHysteriaStreamSettings
{
    [JsonPropertyName("auth")]
    public string? Auth { get; set; }
}

public class XrayFinalmask
{
    [JsonPropertyName("udp")]
    public List<XrayFinalmaskEntry>? Udp { get; set; }

    [JsonPropertyName("tcp")]
    public List<XrayFinalmaskEntry>? Tcp { get; set; }
}

public class XrayFinalmaskEntry
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("settings")]
    public XrayFinalmaskEntrySettings? Settings { get; set; }
}

public class XrayFinalmaskEntrySettings
{
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

public class XraySockopt
{
    [JsonPropertyName("dialerProxy")]
    public string? DialerProxy { get; set; }
}
