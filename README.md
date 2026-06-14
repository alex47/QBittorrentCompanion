# QBittorrentCompanion

QBittorrentCompanion is a small desktop helper that registers itself as the handler for magnet links and `.torrent` files, then forwards them to a qBittorrent Web API instance.

It supports Windows and Linux from one .NET project:

- Windows: Registry protocol/file association plus Windows toast notifications.
- Linux: XDG MIME registration plus desktop notifications through `notify-send`.

## Requirements

- .NET 10 SDK
- A reachable qBittorrent Web UI/API endpoint
- qBittorrent Web UI credentials
- Linux only:
  - `xdg-mime`
  - `xdg-desktop-menu`
  - `notify-send` for desktop notifications, optional but recommended

## Configuration

Edit `QBittorrentCompanion/appsettings.json` before publishing or edit the copied `appsettings.json` in the publish output after publishing.

```json
{
  "QBittorrentConfig": {
    "BaseUrl": "http://192.168.0.11:8081/api/v2/",
    "Username": "admin",
    "Password": "admin",
    "DeleteFileAfterAdding": true
  }
}
```

`BaseUrl` must point to qBittorrent's API root and may include or omit the trailing slash. For example:

```text
http://192.168.0.11:8081/api/v2/
```

Use `https://` only when the qBittorrent endpoint is actually serving HTTPS with a certificate trusted by the OS/.NET runtime.

`DeleteFileAfterAdding` only affects `.torrent` files. Magnet links are never deleted.

## Build

Build all supported targets:

```bash
dotnet build QBittorrentCompanion.sln
```

Build only the Linux target:

```bash
dotnet build QBittorrentCompanion/QBittorrentCompanion.csproj -f net10.0
```

Build only the Windows target:

```bash
dotnet build QBittorrentCompanion/QBittorrentCompanion.csproj -f net10.0-windows10.0.17763.0
```

## Publish

Linux:

```bash
dotnet publish QBittorrentCompanion/QBittorrentCompanion.csproj -f net10.0 -c Release
```

Published app path:

```text
QBittorrentCompanion/bin/Release/net10.0/publish/QBittorrentCompanion
```

Windows:

```powershell
dotnet publish QBittorrentCompanion/QBittorrentCompanion.csproj -f net10.0-windows10.0.17763.0 -c Release
```

## Register As Handler

Run registration from the published executable, not through `dotnet run`.

Linux:

```bash
QBittorrentCompanion/bin/Release/net10.0/publish/QBittorrentCompanion -register
```

This creates:

```text
~/.local/share/applications/qbittorrent-companion.desktop
```

and registers it for:

```text
x-scheme-handler/magnet
application/x-bittorrent
```

Verify Linux registration:

```bash
xdg-mime query default x-scheme-handler/magnet
xdg-mime query default application/x-bittorrent
```

Both should print:

```text
qbittorrent-companion.desktop
```

Windows:

Run the published Windows executable with:

```powershell
.\QBittorrentCompanion.exe -register
```

This registers current-user handlers for magnet links and `.torrent` files under `HKCU\Software\Classes`.

## Usage

After registration, opening a magnet link or `.torrent` file from the desktop environment should launch QBittorrentCompanion automatically.

Manual commands are also supported:

```bash
QBittorrentCompanion -addtorrentmagnetlink "magnet:?xt=..."
QBittorrentCompanion -addtorrentfile "/path/to/file.torrent"
QBittorrentCompanion "magnet:?xt=..."
QBittorrentCompanion "/path/to/file.torrent"
QBittorrentCompanion "file:///path/to/file.torrent"
```

## Troubleshooting

`The SSL connection could not be established`

The configured `BaseUrl` uses HTTPS, but the endpoint is not serving valid HTTPS to this machine. Check whether qBittorrent is actually using HTTPS on that host/port and whether the certificate is trusted.

`Adding torrent failed: Forbidden`

qBittorrent rejected the API request. Common causes are invalid credentials, Web UI host header validation, IP/subnet restrictions, or qBittorrent's failed-login ban settings.

Linux registration says it must be run from the published executable

Publish first, then run `-register` from the apphost in the publish directory. Linux registration stores the executable path in the `.desktop` file, so registering a temporary `dotnet run` path is intentionally blocked.

No desktop notification appears on Linux

Install or configure a notification provider that supports `notify-send`. The app falls back to console output when `notify-send` is unavailable.
