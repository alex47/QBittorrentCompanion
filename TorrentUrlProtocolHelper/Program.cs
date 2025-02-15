internal class Program
{
    private static void Main(string[] args)
    {
        try
        {


            RegisterMagnetProtocol();

            string magnetLink = GetMagnetLinkFromArgs(args);

            AddTorrent(magnetLink);

            string torrentName = ExtractNameFromMagnetLink(magnetLink);
            ShowToastNotification("Torrent added", torrentName);
        }
        catch (Exception ex)
        {
            ShowToastNotification("Error", ex.Message);
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

        static void AddTorrent(string magnetLink)
        {
            var qBittorrentConfig = TorrentUrlProtocolHelper.Configuration.QBittorrentConfig;

            HttpClientHandler httpClientHandler = new() { CookieContainer = new System.Net.CookieContainer() }; ;
            HttpClient httpClient = new(httpClientHandler);

            var loginData = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("username", qBittorrentConfig.Username),
        new KeyValuePair<string, string>("password", qBittorrentConfig.Password)
            ]);

            httpClient.PostAsync($"{qBittorrentConfig.BaseUrl}auth/login", loginData).Wait();

            var addData = new FormUrlEncodedContent([new KeyValuePair<string, string>("urls", magnetLink)]);
            httpClient.PostAsync($"{qBittorrentConfig.BaseUrl}torrents/add", addData).Wait();
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

        static string ExtractNameFromMagnetLink(string magnetLink)
        {
            var uri = new Uri(magnetLink);
            string query = uri.Query;

            var parameters = System.Web.HttpUtility.ParseQueryString(query);
            string? name = parameters["dn"];

            return name is null ? "Could not extract name" : System.Web.HttpUtility.UrlDecode(name);
        }
    }
}