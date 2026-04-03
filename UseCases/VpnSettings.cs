namespace UseCases;

public class VpnSettings
{
    public long[] InboundIds { get; init; }
    public string SubLinkTemplate { get; init; }

    public string Flow { get; init; } = "xtls-rprx-vision";
}