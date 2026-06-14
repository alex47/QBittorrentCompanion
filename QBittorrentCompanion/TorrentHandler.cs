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
        using var httpClientHandler = new HttpClientHandler() { CookieContainer = new System.Net.CookieContainer() };
        using var httpClient = new HttpClient(httpClientHandler);

        Login(httpClient);

        using var addData = new FormUrlEncodedContent([new KeyValuePair<string, string>("urls", magnetLink)]);
        using HttpResponseMessage addResponse = httpClient
            .PostAsync(BuildApiUri("torrents/add"), addData)
            .GetAwaiter()
            .GetResult();

        if (addResponse.IsSuccessStatusCode == false)
        {
            throw new Exception($"Adding torrent failed: {addResponse.StatusCode} - {ReadResponseContent(addResponse)}");
        }
    }

    private static void AddTorrentFileOnWebApi(string filePath)
    {
        using var httpClientHandler = new HttpClientHandler() { CookieContainer = new System.Net.CookieContainer() };
        using var httpClient = new HttpClient(httpClientHandler);

        Login(httpClient);

        using var content = new MultipartFormDataContent();

        var torrentBytes = File.ReadAllBytes(filePath);
        var fileContent = new ByteArrayContent(torrentBytes);

        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-bittorrent");

        content.Add(fileContent, "torrents", Path.GetFileName(filePath));

        using HttpResponseMessage addResponse = httpClient
            .PostAsync(BuildApiUri("torrents/add"), content)
            .GetAwaiter()
            .GetResult();

        if (addResponse.IsSuccessStatusCode == false)
        {
            throw new Exception($"Adding torrent failed: {addResponse.StatusCode} - {ReadResponseContent(addResponse)}");
        }
    }

    private static void Login(HttpClient httpClient)
    {
        using var loginData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("username", Configuration.QBittorrentConfig.Username),
            new KeyValuePair<string, string>("password", Configuration.QBittorrentConfig.Password)
        ]);

        using HttpResponseMessage loginResponse = httpClient
            .PostAsync(BuildApiUri("auth/login"), loginData)
            .GetAwaiter()
            .GetResult();

        if (loginResponse.IsSuccessStatusCode == false)
        {
            throw new Exception($"Login failed: {loginResponse.StatusCode} - {ReadResponseContent(loginResponse)}");
        }
    }

    private static Uri BuildApiUri(string relativePath)
    {
        string baseUrl = Configuration.QBittorrentConfig.BaseUrl;
        if (baseUrl.EndsWith('/') == false)
        {
            baseUrl += "/";
        }

        return new Uri(new Uri(baseUrl, UriKind.Absolute), relativePath);
    }

    private static string ReadResponseContent(HttpResponseMessage response) =>
        response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

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
