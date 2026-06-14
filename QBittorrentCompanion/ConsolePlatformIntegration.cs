namespace QBittorrentCompanion;

internal sealed class ConsolePlatformIntegration : IPlatformIntegration
{
    public void RegisterHandlers() =>
        throw new PlatformNotSupportedException("Handler registration is only supported on Windows and Linux.");

    public void ShowNotification(string title, string message) =>
        Console.WriteLine($"{title}: {message}");

    public void ShowUsage(string usage) =>
        Console.WriteLine(usage);

    public string GetExecutablePathForRegistration() =>
        Environment.ProcessPath ?? throw new InvalidOperationException("Could not determine the current executable path.");
}
