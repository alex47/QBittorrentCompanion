using System.Runtime.InteropServices;

namespace QBittorrentCompanion;

internal static class PlatformIntegrationFactory
{
    public static IPlatformIntegration Create()
    {
#if WINDOWS
        return new WindowsPlatformIntegration();
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxPlatformIntegration();
        }

        return new ConsolePlatformIntegration();
#endif
    }
}
