namespace QBittorrentCompanion;

internal interface IPlatformIntegration
{
    void RegisterHandlers();

    void ShowNotification(string title, string message);

    void ShowUsage(string usage);

    string GetExecutablePathForRegistration();
}
