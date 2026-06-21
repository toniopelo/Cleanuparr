namespace Cleanuparr.Persistence.Models.Configuration.DownloadCleaner;

/// <summary>
/// Marks a configuration object as supporting tag/label-based filtering.
/// Used for clients that expose per-torrent tags: qBittorrent (tags) and Transmission (labels).
/// </summary>
public interface ITagFilterable
{
    /// <summary>
    /// The torrent must have at least one of these tags/labels. Empty = no tag filter.
    /// </summary>
    List<string> TagsAny { get; set; }

    /// <summary>
    /// The torrent must have ALL of these tags/labels. Empty = no tag filter.
    /// </summary>
    List<string> TagsAll { get; set; }
}
