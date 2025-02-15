using TorrentUrlProtocolHelper;

internal class Program
{
    private enum OperationMode
    {
        Unknown,
        AddTorrent,
        RegisterMagnetProtocol,
    }

    private static void Main(string[] args)
    {

        try
        {
            var operationMode = GetOperationModeFromArgs(args);

            switch (operationMode)
            {
                case OperationMode.AddTorrent:
                    string torrentName = TorrentHandler.AddTorrent(args[1]);
                    ShowToastNotification("Torrent added", torrentName);
                    break;

                case OperationMode.RegisterMagnetProtocol:
                    RegisterMagnetProtocol();
                    ShowToastNotification("Registration", "Registration complete.");
                    break;

                default:
                    ShowUsageInfo();
                    break;
            }
        }
        catch (Exception ex)
        {
            ShowToastNotification("Error", ex.Message);
        }
    }

    private static OperationMode GetOperationModeFromArgs(string[] args)
    {
        if (args.Length == 1 && args[0] == "-register")
        {
            return OperationMode.RegisterMagnetProtocol;
        }

        if (args.Length == 2 && args[0] == "-addtorrent")
        {
            return OperationMode.AddTorrent;
        }

        return OperationMode.Unknown;
    }

    static void RegisterMagnetProtocol()
    {
        const string keyPath = $@"Software\Classes\magnet";

        // Delete the key if it already exists
        try
        {
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(keyPath);
        }
        catch { /* Ignore if not exists */ }

        // Create the key
        using var newKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);

        newKey.SetValue("", "URL:Magnet Protocol");
        newKey.SetValue("URL Protocol", "");

        using var registryKeyShell = newKey.CreateSubKey("shell");
        using var registryKeyOpen = registryKeyShell.CreateSubKey("open");
        using var registryKeyCommand = registryKeyOpen.CreateSubKey("command");

        registryKeyCommand.SetValue("", $@"""{Environment.ProcessPath}"" -addtorrent ""%1""");
    }

    private static void ShowUsageInfo()
    {
        const string usageInfo = """
            Usage:
            -register: Register the magnet protocol to use this application.
            -addtorrent <magnet_link>: Add a torrent using the magnet link.
            """;

        MessageBox.Show(usageInfo);
    }

    static void ShowToastNotification(string title, string message)
    {
        string logoFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "qbittorrent_logo.png");

        new Microsoft.Toolkit.Uwp.Notifications.ToastContentBuilder()
            .AddAppLogoOverride(new Uri(logoFilePath))
            .AddText(title)
            .AddText(message)
            .Show();
    }
}