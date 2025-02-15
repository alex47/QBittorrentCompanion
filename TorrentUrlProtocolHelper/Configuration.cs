using Microsoft.Extensions.Configuration;

namespace TorrentUrlProtocolHelper;

internal class Configuration
{
    public static QBittorrentConfig QBittorrentConfig => Config.GetSection("QBittorrentConfig").Get<QBittorrentConfig>()!;

    private static IConfiguration Config { get; }

    static Configuration()
    {
        Config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}
