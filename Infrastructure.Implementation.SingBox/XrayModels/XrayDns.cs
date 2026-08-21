using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Implementation.SingBox.XrayModels;

public class XrayDns
{
    [JsonPropertyName("servers")]
    [JsonConverter(typeof(XrayDnsServerListConverter))]
    public List<XrayDnsServer> Servers { get; set; } = [];
}

public class XrayDnsServer
{
    public string? Address { get; set; }

    public List<string> Domains { get; set; } = [];
}

public class XrayDnsServerListConverter : JsonConverter<List<XrayDnsServer>>
{
    public override List<XrayDnsServer> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new List<XrayDnsServer>();
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            reader.Skip();
            return result;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                result.Add(new XrayDnsServer { Address = element.GetString() });
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                var address = element.TryGetProperty("address", out var addressElement)
                    ? addressElement.GetString()
                    : null;

                var domains = new List<string>();
                if (element.TryGetProperty("domains", out var domainsElement) &&
                    domainsElement.ValueKind == JsonValueKind.Array)
                {
                    domains.AddRange(domainsElement.EnumerateArray()
                        .Where(d => d.ValueKind == JsonValueKind.String)
                        .Select(d => d.GetString()!));
                }

                result.Add(new XrayDnsServer { Address = address, Domains = domains });
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<XrayDnsServer> value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Xray models are read-only (deserialize-only).");
    }
}
