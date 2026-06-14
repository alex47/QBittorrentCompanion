using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace QBittorrentCompanion;

internal sealed class LinuxPlatformIntegration : IPlatformIntegration
{
    private const string DesktopFileName = "qbittorrent-companion.desktop";
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public void RegisterHandlers()
    {
        string executablePath = GetExecutablePathForRegistration();
        string applicationsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "applications");
        string desktopFilePath = Path.Combine(applicationsDirectory, DesktopFileName);

        Directory.CreateDirectory(applicationsDirectory);
        File.WriteAllText(desktopFilePath, CreateDesktopEntry(executablePath), Utf8NoBom);

        RegisterMimeDefault("x-scheme-handler/magnet");
        RegisterMimeDefault("application/x-bittorrent");
        TryRun("update-desktop-database", applicationsDirectory);
    }

    public void ShowNotification(string title, string message)
    {
        if (TryRun("notify-send", title, message) == false)
        {
            Console.WriteLine($"{title}: {message}");
        }
    }

    public void ShowUsage(string usage) =>
        Console.WriteLine(usage);

    public string GetExecutablePathForRegistration()
    {
        string executablePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Could not determine the current executable path.");

        string executableName = Path.GetFileNameWithoutExtension(executablePath);
        if (string.Equals(executableName, "dotnet", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Linux registration must be run from the published QBittorrentCompanion executable, not through dotnet. Run dotnet publish first, then run the published app with -register.");
        }

        if (File.Exists(executablePath) == false)
        {
            throw new InvalidOperationException($"The current executable path does not exist: {executablePath}");
        }

        return executablePath;
    }

    private static string CreateDesktopEntry(string executablePath)
    {
        string iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "qbittorrent_logo.png");

        return $"""
            [Desktop Entry]
            Type=Application
            Name=QBittorrent Companion
            Comment=Add torrents to qBittorrent through the Web API
            Exec={QuoteDesktopExecPath(executablePath)} %u
            Icon={iconPath}
            Terminal=false
            NoDisplay=true
            MimeType=x-scheme-handler/magnet;application/x-bittorrent;
            Categories=Network;FileTransfer;
            """;
    }

    private static string QuoteDesktopExecPath(string path) =>
        "\"" + path.Replace("\\", "\\\\").Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

    private static void RegisterMimeDefault(string mimeType) =>
        RunRequired("xdg-mime", "default", DesktopFileName, mimeType);

    private static void RunRequired(string fileName, params string[] arguments)
    {
        ProcessResult result = Run(fileName, arguments);
        if (result.ExitCode != 0)
        {
            string error = string.IsNullOrWhiteSpace(result.Error)
                ? result.Output
                : result.Error;

            throw new InvalidOperationException($"{fileName} failed with exit code {result.ExitCode}: {error.Trim()}");
        }
    }

    private static bool TryRun(string fileName, params string[] arguments)
    {
        try
        {
            return Run(fileName, arguments).ExitCode == 0;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    private static ProcessResult Run(string fileName, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {fileName}.");

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ProcessResult(process.ExitCode, output, error);
    }

    private readonly record struct ProcessResult(int ExitCode, string Output, string Error);
}
