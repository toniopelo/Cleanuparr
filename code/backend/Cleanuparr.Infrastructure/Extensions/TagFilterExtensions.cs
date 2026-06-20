using Cleanuparr.Persistence.Models.Configuration.DownloadCleaner;

namespace Cleanuparr.Infrastructure.Extensions;

public static class TagFilterExtensions
{
    public static bool MatchesTags(this ITagFilterable filter, IReadOnlyList<string> tags)
    {
        if (filter.TagsAny.Count > 0 &&
            !filter.TagsAny.Any(t => tags.Contains(t, StringComparer.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (filter.TagsAll.Count > 0 &&
            !filter.TagsAll.All(t => tags.Contains(t, StringComparer.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }
}
