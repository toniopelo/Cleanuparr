using Cleanuparr.Domain.Entities;
using Cleanuparr.Domain.Enums;
using Cleanuparr.Infrastructure.Extensions;
using Cleanuparr.Infrastructure.Services.Interfaces;
using Cleanuparr.Persistence.Models.Configuration.DownloadCleaner;

namespace Cleanuparr.Infrastructure.Services;

public class SeedingRuleEvaluator : ISeedingRuleEvaluator
{
    public ISeedingRule? GetMatchingRule(ITorrentItemWrapper torrent, IEnumerable<ISeedingRule> rules)
    {
        return rules
            .OrderBy(r => r.Priority)
            .FirstOrDefault(rule => Matches(torrent, rule));
    }

    private static bool Matches(ITorrentItemWrapper torrent, ISeedingRule rule)
    {
        // Category check
        if (!rule.Categories.Any(c => c.Equals(torrent.Category ?? string.Empty, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Tracker check
        if (rule.TrackerPatterns.Count > 0)
        {
            bool hasMatchingTracker = torrent.TrackerDomains.Any(domain =>
                rule.TrackerPatterns.Any(pattern =>
                    domain.EndsWith(pattern, StringComparison.OrdinalIgnoreCase)));

            if (!hasMatchingTracker)
            {
                return false;
            }
        }

        // Tag/label check
        if (rule is ITagFilterable tagFilterable && !tagFilterable.MatchesTags(torrent.Tags))
        {
            return false;
        }

        // Privacy check
        return rule.PrivacyType switch
        {
            TorrentPrivacyType.Public => !torrent.IsPrivate,
            TorrentPrivacyType.Private => torrent.IsPrivate,
            _ => true
        };
    }
}
