namespace QBittorrentCompanion;

internal class TorrentHandler
{
    public static string AddMagnetLink(string magnetLink)
    {
        AddMagnetLinkOnWebApi(magnetLink);

        return ExtractNameFromMagnetLink(magnetLink);
    }

    public static string AddTorrentFile(string filePath)
    {
        AddTorrentFileOnWebApi(filePath);

        string torrentName = ExtractNameFromTorrentFile(filePath);

        if (Configuration.QBittorrentConfig.DeleteFileAfterAdding)
        {
            File.Delete(filePath);
        }

        return torrentName;
    }

    private static void AddMagnetLinkOnWebApi(string magnetLink)
    {
        var httpClientHandler = new HttpClientHandler() { CookieContainer = new System.Net.CookieContainer() }; ;
        var httpClient = new HttpClient(httpClientHandler);

        var loginData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("username", Configuration.QBittorrentConfig.Username),
            new KeyValuePair<string, string>("password", Configuration.QBittorrentConfig.Password)
        ]);

        var loginResponse = httpClient.PostAsync($"{Configuration.QBittorrentConfig.BaseUrl}auth/login", loginData).Result;

        if (loginResponse.IsSuccessStatusCode == false)
        {
            throw new Exception($"Login failed: {loginResponse.StatusCode}");
        }

        var addData = new FormUrlEncodedContent([new KeyValuePair<string, string>("urls", magnetLink)]);

        var addResponse = httpClient.PostAsync($"{Configuration.QBittorrentConfig.BaseUrl}torrents/add", addData).Result;

        if (addResponse.IsSuccessStatusCode == false)
        {
            throw new Exception($"Adding torrent failed: {addResponse.StatusCode}");
        }
    }

    private static void AddTorrentFileOnWebApi(string filePath)
    {
        var httpClientHandler = new HttpClientHandler() { CookieContainer = new System.Net.CookieContainer() };
        var httpClient = new HttpClient(httpClientHandler);

        var loginData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("username", Configuration.QBittorrentConfig.Username),
            new KeyValuePair<string, string>("password", Configuration.QBittorrentConfig.Password)
        ]);

        var loginResponse = httpClient.PostAsync($"{Configuration.QBittorrentConfig.BaseUrl}auth/login", loginData).Result;

        if (loginResponse.IsSuccessStatusCode == false)
        {
            throw new Exception($"Login failed: {loginResponse.StatusCode}");
        }

        using var content = new MultipartFormDataContent();

        var torrentBytes = File.ReadAllBytes(filePath);
        var fileContent = new ByteArrayContent(torrentBytes);

        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-bittorrent");

        content.Add(fileContent, "torrents", Path.GetFileName(filePath));

        var addResponse = httpClient.PostAsync($"{Configuration.QBittorrentConfig.BaseUrl}torrents/add", content).Result;

        if (addResponse.IsSuccessStatusCode == false)
        {
            string error = addResponse.Content.ReadAsStringAsync().Result;
            throw new Exception($"Adding torrent failed: {addResponse.StatusCode} - {error}");
        }
    }

    private static string ExtractNameFromMagnetLink(string magnetLink)
    {
        var uri = new Uri(magnetLink);
        string query = uri.Query;

        var parameters = System.Web.HttpUtility.ParseQueryString(query);
        string? name = parameters["dn"];

        return name is null ? "*Could not extract torrent name*" : System.Web.HttpUtility.UrlDecode(name);
    }

    private static string ExtractNameFromTorrentFile(string filePath) => Path.GetFileNameWithoutExtension(filePath);
}
