namespace QBittorrentCompanion;

internal class TorrentHandler
{
    public static string AddTorrent(string magnetLink)
    {
        AddTorrentOnWebApi(magnetLink);

        return ExtractNameFromMagnetLink(magnetLink);
    }

    private static void AddTorrentOnWebApi(string magnetLink)
    {
        var qBittorrentConfig = Configuration.QBittorrentConfig;

        var httpClientHandler = new HttpClientHandler() { CookieContainer = new System.Net.CookieContainer() }; ;
        var httpClient = new HttpClient(httpClientHandler);

        var loginData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("username", qBittorrentConfig.Username),
            new KeyValuePair<string, string>("password", qBittorrentConfig.Password)
        ]);

        var loginResponse = httpClient.PostAsync($"{qBittorrentConfig.BaseUrl}auth/login", loginData).Result;

        if (loginResponse.IsSuccessStatusCode == false)
        {
            throw new Exception($"Login failed: {loginResponse.StatusCode}");
        }

        var addData = new FormUrlEncodedContent([new KeyValuePair<string, string>("urls", magnetLink)]);

        var addResponse = httpClient.PostAsync($"{qBittorrentConfig.BaseUrl}torrents/add", addData).Result;

        if (addResponse.IsSuccessStatusCode == false)
        {
            throw new Exception($"Adding torrent failed: {addResponse.StatusCode}");
        }
    }

    static string ExtractNameFromMagnetLink(string magnetLink)
    {
        var uri = new Uri(magnetLink);
        string query = uri.Query;

        var parameters = System.Web.HttpUtility.ParseQueryString(query);
        string? name = parameters["dn"];

        return name is null ? "*Could not extract torrent name*" : System.Web.HttpUtility.UrlDecode(name);
    }
}
