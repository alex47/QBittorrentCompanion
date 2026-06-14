using QBittorrentCompanion;

internal class Program
{
    internal const string OptionAddTorrentMagnetLink = "-addtorrentmagnetlink";
    internal const string OptionAddTorrentFile = "-addtorrentfile";
    internal const string OptionRegister = "-register";

    private const string UsageInfo = """
        Usage:
        -register: Register the magnet protocol and torrent file association to use this application.
        -addtorrentmagnetlink <magnet_link>: Add a torrent using the magnet link.
        -addtorrentfile <torrent_file_path>: Add a torrent using the torrent file.
        <magnet_link>: Add a torrent using the magnet link.
        <torrent_file_path>: Add a torrent using the torrent file.
        """;

    private enum OperationMode
    {
        Unknown,
        AddTorrentMagnetLink,
        AddTorrentFile,
        RegisterHandlers,
    }

    private readonly record struct ParsedOperation(OperationMode Mode, string? Value);

    private static void Main(string[] args)
    {
        IPlatformIntegration platformIntegration = PlatformIntegrationFactory.Create();

        try
        {
            ParsedOperation operation = GetOperationFromArgs(args);

            switch (operation.Mode)
            {
                case OperationMode.AddTorrentMagnetLink:
                    string magnetTorrentName = TorrentHandler.AddMagnetLink(operation.Value!);
                    platformIntegration.ShowNotification("Torrent added", magnetTorrentName);
                    break;

                case OperationMode.AddTorrentFile:
                    string fileTorrentName = TorrentHandler.AddTorrentFile(operation.Value!);
                    platformIntegration.ShowNotification("Torrent added", fileTorrentName);
                    break;

                case OperationMode.RegisterHandlers:
                    platformIntegration.RegisterHandlers();
                    platformIntegration.ShowNotification("Registration", "Registration complete.");
                    break;

                default:
                    platformIntegration.ShowUsage(UsageInfo);
                    break;
            }
        }
        catch (Exception ex)
        {
            platformIntegration.ShowNotification("Error", ex.Message);
        }
    }

    private static ParsedOperation GetOperationFromArgs(string[] args)
    {
        if (args.Length == 1 && string.Equals(args[0], OptionRegister, StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedOperation(OperationMode.RegisterHandlers, null);
        }

        if (args.Length == 2 && string.Equals(args[0], OptionAddTorrentMagnetLink, StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedOperation(OperationMode.AddTorrentMagnetLink, args[1]);
        }

        if (args.Length == 2 && string.Equals(args[0], OptionAddTorrentFile, StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedOperation(OperationMode.AddTorrentFile, NormalizeTorrentFileArgument(args[1]));
        }

        if (args.Length == 1)
        {
            if (IsMagnetLink(args[0]))
            {
                return new ParsedOperation(OperationMode.AddTorrentMagnetLink, args[0]);
            }

            if (TryNormalizeTorrentFileArgument(args[0], out string? torrentFilePath))
            {
                return new ParsedOperation(OperationMode.AddTorrentFile, torrentFilePath);
            }
        }

        return new ParsedOperation(OperationMode.Unknown, null);
    }

    private static bool IsMagnetLink(string value) =>
        value.StartsWith("magnet:?", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeTorrentFileArgument(string value)
    {
        if (TryNormalizeTorrentFileArgument(value, out string? torrentFilePath) && torrentFilePath is not null)
        {
            return torrentFilePath;
        }

        return value;
    }

    private static bool TryNormalizeTorrentFileArgument(string value, out string? torrentFilePath)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && uri.IsFile)
        {
            string localPath = uri.LocalPath;
            if (HasTorrentExtension(localPath))
            {
                torrentFilePath = localPath;
                return true;
            }

            torrentFilePath = null;
            return false;
        }

        if (HasTorrentExtension(value))
        {
            torrentFilePath = value;
            return true;
        }

        torrentFilePath = null;
        return false;
    }

    private static bool HasTorrentExtension(string value) =>
        string.Equals(Path.GetExtension(value), ".torrent", StringComparison.OrdinalIgnoreCase);
}
