#if WINDOWS
using Microsoft.Win32;
using Microsoft.Toolkit.Uwp.Notifications;

namespace QBittorrentCompanion;

internal sealed class WindowsPlatformIntegration : IPlatformIntegration
{
    public void RegisterHandlers()
    {
        RegisterTorrentMagnetLinkProtocol();
        RegisterTorrentFileAssociation();
    }

    public void ShowNotification(string title, string message)
    {
        string logoFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "qbittorrent_logo.png");

        new ToastContentBuilder()
            .AddAppLogoOverride(new Uri(logoFilePath))
            .AddText(title)
            .AddText(message)
            .Show();
    }

    public void ShowUsage(string usage) =>
        System.Windows.Forms.MessageBox.Show(usage);

    public string GetExecutablePathForRegistration() =>
        Environment.ProcessPath ?? throw new InvalidOperationException("Could not determine the current executable path.");

    private void RegisterTorrentMagnetLinkProtocol()
    {
        const string keyPath = @"Software\Classes\magnet";

        TryDeleteCurrentUserRegistryTree(keyPath);

        using RegistryKey newKey = Registry.CurrentUser.CreateSubKey(keyPath);

        newKey.SetValue("", "URL:Magnet Protocol");
        newKey.SetValue("URL Protocol", "");

        using RegistryKey registryKeyShell = newKey.CreateSubKey("shell");
        using RegistryKey registryKeyOpen = registryKeyShell.CreateSubKey("open");
        using RegistryKey registryKeyCommand = registryKeyOpen.CreateSubKey("command");

        registryKeyCommand.SetValue("", $@"""{GetExecutablePathForRegistration()}"" {Program.OptionAddTorrentMagnetLink} ""%1""");
    }

    private void RegisterTorrentFileAssociation()
    {
        const string progId = "TorrentUrlProtocolHelper.TorrentFile";

        TryDeleteCurrentUserRegistryTree(@"Software\Classes\.torrent");
        TryDeleteCurrentUserRegistryTree($@"Software\Classes\{progId}");

        using (RegistryKey extKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.torrent"))
        {
            extKey.SetValue("", progId);
        }

        using RegistryKey progIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}");
        progIdKey.SetValue("", "Torrent File");

        using (RegistryKey defaultIconKey = progIdKey.CreateSubKey("DefaultIcon"))
        {
            defaultIconKey.SetValue("", $@"""{GetExecutablePathForRegistration()}"",0");
        }

        using RegistryKey shellKey = progIdKey.CreateSubKey("shell");
        using RegistryKey openKey = shellKey.CreateSubKey("open");
        using RegistryKey commandKey = openKey.CreateSubKey("command");

        commandKey.SetValue("", $@"""{GetExecutablePathForRegistration()}"" {Program.OptionAddTorrentFile} ""%1""");
    }

    private static void TryDeleteCurrentUserRegistryTree(string keyPath)
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(keyPath);
        }
        catch
        {
            // Ignore missing keys to preserve the original registration behavior.
        }
    }
}
#endif
