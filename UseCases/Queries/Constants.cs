namespace UseCases.Queries;

public class Constants
{
    public const string DirectName = "Отписчик :(";

    public const string DirectOnlyXrayJson =
        """[{"remarks":"Отключено","outbounds":[{"tag":"Отписчик :("","protocol":"freedom","settings":{}}]}]""";

    public const string DirectOnlySingBoxJson = """
                                                {
                                                  "outbounds":   [
                                                    {
                                                      "type": "vless",
                                                      "tag": "Отписчик :(",
                                                      "server": "0.0.0.0",
                                                      "server_port": 1,
                                                      "uuid": "00000000-0000-0000-0000-000000000000",
                                                      "packet_encoding": "xudp"
                                                    }
                                                  ],
                                                  "endpoints":   [
                                                  ]
                                                }
                                                """;
}