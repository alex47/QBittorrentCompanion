RegisterMagnetProtocol();
string magnetLink = GetMagnetLinkFromArgs(args);
await AddTorrentAsync(magnetLink);


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

    registryKeyCommand.SetValue("", $@"""{Environment.ProcessPath}"" ""%1""");
}

static string GetMagnetLinkFromArgs(string[] args)
{
    if (args.Length == 0)
    {
        throw new ArgumentException("No magnet link provided as argument.");
    }

    string magnetLink = args[0];

    if (magnetLink.StartsWith("magnet:") == false)
    {
        throw new ArgumentException("Invalid magnet link provided as argument.");
    }

    return magnetLink;
}

static async Task AddTorrentAsync(string magnetLink)
{
    const string BaseUrl = "http://192.168.0.11:8081/api/v2/";

    HttpClientHandler httpClientHandler = new() { CookieContainer = new System.Net.CookieContainer() }; ;
    HttpClient httpClient = new(httpClientHandler);

    var loginData = new FormUrlEncodedContent(
    [
        new KeyValuePair<string, string>("username", "admin"),
            new KeyValuePair<string, string>("password", "admin")
    ]);

    await httpClient.PostAsync(BaseUrl + "auth/login", loginData);

    var addData = new FormUrlEncodedContent([new KeyValuePair<string, string>("urls", magnetLink)]);
    await httpClient.PostAsync($"{BaseUrl}torrents/add", addData);
}
