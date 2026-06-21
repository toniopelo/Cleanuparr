using Cleanuparr.Domain.Enums;

namespace Cleanuparr.Api.Features.DownloadCleaner;

internal static class DownloadCleanerClientCapabilities
{
    public static bool SupportsTagFilters(DownloadClientTypeName typeName)
        => typeName is DownloadClientTypeName.qBittorrent or DownloadClientTypeName.Transmission;
}
