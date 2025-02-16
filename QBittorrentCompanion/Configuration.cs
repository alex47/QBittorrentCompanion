using Microsoft.Extensions.Configuration;

namespace QBittorrentCompanion;

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

public class QBittorrentConfig
{
    public required string BaseUrl { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }

    public required bool DeleteFileAfterAdding { get; set; }
}