namespace Cleanuparr.Api.Features.DownloadCleaner;

internal static class DownloadCleanerRequestSanitizer
{
    public static List<string> SanitizeStringList(List<string> list)
        => list.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
}
