using Cleanuparr.Api.Features.DownloadCleaner.Contracts.Requests;
using Cleanuparr.Api.Features.DownloadCleaner.Contracts.Responses;
using Cleanuparr.Api.Features.DownloadCleaner.Controllers;
using Cleanuparr.Api.Tests.Features.DownloadCleaner.TestHelpers;
using Cleanuparr.Api.Tests.TestHelpers;
using Cleanuparr.Domain.Enums;
using Cleanuparr.Persistence;
using Cleanuparr.Persistence.Models.Configuration.DownloadCleaner;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Cleanuparr.Api.Tests.Features.DownloadCleaner;

public class UnlinkedConfigControllerTests : IDisposable
{
    private readonly DataContext _dataContext;
    private readonly UnlinkedConfigController _controller;

    public UnlinkedConfigControllerTests()
    {
        _dataContext = SeedingRulesTestDataFactory.CreateDataContext();
        var logger = Substitute.For<ILogger<UnlinkedConfigController>>();
        _controller = new UnlinkedConfigController(logger, _dataContext);
        ControllerTestContext.Attach(_controller);
    }

    public void Dispose()
    {
        _dataContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static UnlinkedConfigRequest ValidRequest(
        List<string>? categories = null,
        List<string>? tagsAny = null,
        List<string>? tagsAll = null)
        => new()
        {
            Enabled = true,
            TargetCategory = "cleanuparr-unlinked",
            Categories = categories ?? ["movies"],
            TagsAny = tagsAny ?? [],
            TagsAll = tagsAll ?? [],
        };

    [Fact]
    public async Task Update_ValidRequest_PersistsTagFilters()
    {
        var client = SeedingRulesTestDataFactory.AddDownloadClient(_dataContext);

        var result = await _controller.UpdateUnlinkedConfig(client.Id, ValidRequest(
            categories: ["movies", "tv"],
            tagsAny: ["radarr-imported"],
            tagsAll: ["arr-imported"]));

        result.ShouldBeOfType<OkObjectResult>();
        var saved = await _dataContext.UnlinkedConfigs.AsNoTracking().SingleAsync(u => u.DownloadClientConfigId == client.Id);
        saved.Categories.ShouldBe(new List<string> { "movies", "tv" });
        saved.TagsAny.ShouldBe(new List<string> { "radarr-imported" });
        saved.TagsAll.ShouldBe(new List<string> { "arr-imported" });
    }

    [Fact]
    public async Task Update_ThenGet_RoundTripsTagFilters()
    {
        var client = SeedingRulesTestDataFactory.AddDownloadClient(_dataContext);
        await _controller.UpdateUnlinkedConfig(client.Id, ValidRequest(
            tagsAny: ["radarr-imported"],
            tagsAll: ["arr-imported"]));

        var result = await _controller.GetUnlinkedConfig(client.Id);

        var ok = result.ShouldBeOfType<OkObjectResult>();
        var config = ok.Value.ShouldBeOfType<UnlinkedConfigResponse>();
        config.TagsAny.ShouldBe(new List<string> { "radarr-imported" });
        config.TagsAll.ShouldBe(new List<string> { "arr-imported" });
    }

    [Fact]
    public async Task Update_TagFilters_SanitizesWhitespaceAndDropsEmptyEntries()
    {
        var client = SeedingRulesTestDataFactory.AddDownloadClient(_dataContext);

        var result = await _controller.UpdateUnlinkedConfig(client.Id,
            ValidRequest(
                tagsAny: [" radarr-imported ", "", "  "],
                tagsAll: [" arr-imported "]));

        result.ShouldBeOfType<OkObjectResult>();
        var saved = await _dataContext.UnlinkedConfigs.AsNoTracking().SingleAsync(u => u.DownloadClientConfigId == client.Id);
        saved.TagsAny.ShouldBe(new List<string> { "radarr-imported" });
        saved.TagsAll.ShouldBe(new List<string> { "arr-imported" });
    }

    [Fact]
    public async Task Update_ForUnsupportedClient_ClearsTagFilters()
    {
        var client = SeedingRulesTestDataFactory.AddDownloadClient(_dataContext, DownloadClientTypeName.Deluge, "Test Deluge");

        var result = await _controller.UpdateUnlinkedConfig(client.Id, ValidRequest(
            tagsAny: ["radarr-imported"],
            tagsAll: ["arr-imported"]));

        var ok = result.ShouldBeOfType<OkObjectResult>();
        var response = ok.Value.ShouldBeOfType<UnlinkedConfigResponse>();
        response.TagsAny.ShouldBeEmpty();
        response.TagsAll.ShouldBeEmpty();

        var saved = await _dataContext.UnlinkedConfigs.AsNoTracking().SingleAsync(u => u.DownloadClientConfigId == client.Id);
        saved.TagsAny.ShouldBeEmpty();
        saved.TagsAll.ShouldBeEmpty();
    }

    [Fact]
    public async Task Get_ForUnsupportedClient_NormalizesStaleTagFilters()
    {
        var client = SeedingRulesTestDataFactory.AddDownloadClient(_dataContext, DownloadClientTypeName.uTorrent, "Test uTorrent");
        _dataContext.UnlinkedConfigs.Add(new UnlinkedConfig
        {
            DownloadClientConfigId = client.Id,
            Enabled = true,
            TargetCategory = "cleanuparr-unlinked",
            Categories = ["movies"],
            TagsAny = ["stale-tag"],
            TagsAll = ["another-stale-tag"],
        });
        await _dataContext.SaveChangesAsync();

        var result = await _controller.GetUnlinkedConfig(client.Id);

        var ok = result.ShouldBeOfType<OkObjectResult>();
        var response = ok.Value.ShouldBeOfType<UnlinkedConfigResponse>();
        response.TagsAny.ShouldBeEmpty();
        response.TagsAll.ShouldBeEmpty();
    }
}
