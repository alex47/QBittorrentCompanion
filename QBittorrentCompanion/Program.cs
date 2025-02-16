using QBittorrentCompanion;

internal class Program
{
    private enum OperationMode
    {
        Unknown,
        AddTorrentMagnetLink,
        AddTorrentFile,
        RegisterMagnetProtocol,
    }

    private const string OptionAddTorrentMagnetLink = "-addtorrentmagnetlink";
    private const string OptionAddTorrentFile = "-addtorrentfile";
    private const string OptionRegister = "-register";

    private static void Main(string[] args)
    {

        try
        {
            var operationMode = GetOperationModeFromArgs(args);
            string torrentName = string.Empty;

            switch (operationMode)
            {
                case OperationMode.AddTorrentMagnetLink:
                    torrentName = TorrentHandler.AddMagnetLink(args[1]);
                    ShowToastNotification("Torrent added", torrentName);
                    break;

                case OperationMode.AddTorrentFile:
                    torrentName = TorrentHandler.AddTorrentFile(args[1]);
                    ShowToastNotification("Torrent added", torrentName);
                    break;

                case OperationMode.RegisterMagnetProtocol:
                    RegisterTorrentMagnetLinkProtocol();
                    RegisterTorrentFileAssociation();
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
        if (args.Length == 1 && args[0] == OptionRegister)
        {
            return OperationMode.RegisterMagnetProtocol;
        }

        if (args.Length == 2 && args[0] == OptionAddTorrentMagnetLink)
        {
            return OperationMode.AddTorrentMagnetLink;
        }

        if (args.Length == 2 && args[0] == OptionAddTorrentFile)
        {
            return OperationMode.AddTorrentFile;
        }

        return OperationMode.Unknown;
    }

    private static void RegisterTorrentMagnetLinkProtocol()
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

        registryKeyCommand.SetValue("", $@"""{Environment.ProcessPath}"" {OptionAddTorrentMagnetLink} ""%1""");
    }

    private static void RegisterTorrentFileAssociation()
    {
        const string progId = "TorrentUrlProtocolHelper.TorrentFile";

        try
        {
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\.torrent");
        }
        catch { /* Ignore if not exists */ }
        try
        {
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{progId}");
        }
        catch { /* Ignore if not exists */ }

        using (var extKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\.torrent"))
        {
            extKey.SetValue("", progId);
        }

        using var progIdKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}");
        progIdKey.SetValue("", "Torrent File");

        // Optionally, set the default icon.
        using (var defaultIconKey = progIdKey.CreateSubKey("DefaultIcon"))
        {
            defaultIconKey.SetValue("", $@"""{Environment.ProcessPath}"",0");
        }

        // Create the shell open command.
        using var shellKey = progIdKey.CreateSubKey("shell");
        using var openKey = shellKey.CreateSubKey("open");
        using var commandKey = openKey.CreateSubKey("command");

        commandKey.SetValue("", $@"""{Environment.ProcessPath}"" {OptionAddTorrentFile} ""%1""");
    }

    private static void ShowUsageInfo()
    {
        const string usageInfo = """
            Usage:
            -register: Register the magnet protocol to use this application.
            -addtorrentmagnetlink <magnet_link>: Add a torrent using the magnet link.
            -addtorrentfile <torrent_file_path>: Add a torrent using the torrent file.
            """;

        MessageBox.Show(usageInfo);
    }

    private static void ShowToastNotification(string title, string message)
    {
        string logoFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "qbittorrent_logo.png");

        new Microsoft.Toolkit.Uwp.Notifications.ToastContentBuilder()
            .AddAppLogoOverride(new Uri(logoFilePath))
            .AddText(title)
            .AddText(message)
            .Show();
    }
}