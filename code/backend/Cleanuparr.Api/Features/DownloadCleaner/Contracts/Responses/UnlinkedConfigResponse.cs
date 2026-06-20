using Cleanuparr.Domain.Enums;
using Cleanuparr.Persistence.Models.Configuration.DownloadCleaner;

namespace Cleanuparr.Api.Features.DownloadCleaner.Contracts.Responses;

public sealed record UnlinkedConfigResponse
{
    public bool Enabled { get; init; }

    public required string TargetCategory { get; init; }

    public bool UseTag { get; init; }

    public required List<string> IgnoredRootDirs { get; init; }

    public required List<string> Categories { get; init; }

    public required List<string> TagsAny { get; init; }

    public required List<string> TagsAll { get; init; }

    public static UnlinkedConfigResponse From(UnlinkedConfig config, DownloadClientTypeName typeName)
    {
        bool supportsTagFilters = SupportsTagFilters(typeName);

        return new UnlinkedConfigResponse
        {
            Enabled = config.Enabled,
            TargetCategory = config.TargetCategory,
            UseTag = config.UseTag,
            IgnoredRootDirs = config.IgnoredRootDirs,
            Categories = config.Categories,
            TagsAny = supportsTagFilters ? config.TagsAny : [],
            TagsAll = supportsTagFilters ? config.TagsAll : [],
        };
    }

    private static bool SupportsTagFilters(DownloadClientTypeName typeName)
        => typeName is DownloadClientTypeName.qBittorrent or DownloadClientTypeName.Transmission;
}
