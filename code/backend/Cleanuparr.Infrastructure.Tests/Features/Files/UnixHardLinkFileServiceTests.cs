using System.Runtime.InteropServices;
using Cleanuparr.Infrastructure.Features.Files;
using Microsoft.Extensions.Logging.Abstractions;
using Mono.Unix.Native;
using Shouldly;
using Xunit;

namespace Cleanuparr.Infrastructure.Tests.Features.Files;

public sealed class UnixHardLinkFileServiceTests : IDisposable
{
    private readonly string _tempRoot;

    public UnixHardLinkFileServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "cleanuparr-hardlinks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public void GetHardLinkCount_WithoutIgnoredRoot_ReturnsZeroForSingleton()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var sut = CreateSut();
        string downloadFile = CreateFile("downloads/file.mkv");

        sut.GetHardLinkCount(downloadFile, ignoreRootDir: false).ShouldBe(0);
    }

    [Fact]
    public void GetHardLinkCount_WithoutIgnoredRoot_ReturnsOneForHardlinkedFile()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var sut = CreateSut();
        string downloadFile = CreateFile("downloads/file.mkv");
        CreateHardLink("library/file.mkv", downloadFile);

        sut.GetHardLinkCount(downloadFile, ignoreRootDir: false).ShouldBe(1);
    }

    [Fact]
    public void GetHardLinkCount_WithIgnoredRoot_ReturnsZeroForSingleton()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var sut = CreateSut();
        string ignoredRoot = CreateDirectory("ignored");
        string downloadFile = CreateFile("downloads/file.mkv");

        sut.PopulateFileCounts(ignoredRoot);

        sut.GetHardLinkCount(downloadFile, ignoreRootDir: true).ShouldBe(0);
    }

    [Fact]
    public void GetHardLinkCount_WithIgnoredRoot_ReturnsZeroForIgnoredRootOnlyHardlink()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var sut = CreateSut();
        string ignoredRoot = CreateDirectory("ignored");
        string downloadFile = CreateFile("downloads/file.mkv");
        CreateHardLink("ignored/file.mkv", downloadFile);

        sut.PopulateFileCounts(ignoredRoot);

        sut.GetHardLinkCount(downloadFile, ignoreRootDir: true).ShouldBe(0);
    }

    [Fact]
    public void GetHardLinkCount_WithIgnoredRoot_ReturnsOneForLibraryHardlinkOnly()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var sut = CreateSut();
        string ignoredRoot = CreateDirectory("ignored");
        string downloadFile = CreateFile("downloads/file.mkv");
        CreateHardLink("library/file.mkv", downloadFile);

        sut.PopulateFileCounts(ignoredRoot);

        sut.GetHardLinkCount(downloadFile, ignoreRootDir: true).ShouldBe(1);
    }

    [Fact]
    public void GetHardLinkCount_WithIgnoredRoot_ReturnsOneForLibraryAndIgnoredRootHardlinks()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var sut = CreateSut();
        string ignoredRoot = CreateDirectory("ignored");
        string downloadFile = CreateFile("downloads/file.mkv");
        CreateHardLink("ignored/file.mkv", downloadFile);
        CreateHardLink("library/file.mkv", downloadFile);

        sut.PopulateFileCounts(ignoredRoot);

        sut.GetHardLinkCount(downloadFile, ignoreRootDir: true).ShouldBe(1);
    }

    [Fact]
    public void GetHardLinkCount_WithIgnoredRoot_ReturnsZeroForMultipleIgnoredRootHardlinks()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var sut = CreateSut();
        string ignoredRoot = CreateDirectory("ignored");
        string downloadFile = CreateFile("downloads/file.mkv");
        CreateHardLink("ignored/file.mkv", downloadFile);
        CreateHardLink("ignored/nested/file.mkv", downloadFile);

        sut.PopulateFileCounts(ignoredRoot);

        sut.GetHardLinkCount(downloadFile, ignoreRootDir: true).ShouldBe(0);
    }

    [Fact]
    public void GetHardLinkCount_WithIgnoredRoot_ReturnsOneForLibraryAndMultipleIgnoredRootHardlinks()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var sut = CreateSut();
        string ignoredRoot = CreateDirectory("ignored");
        string downloadFile = CreateFile("downloads/file.mkv");
        CreateHardLink("ignored/file.mkv", downloadFile);
        CreateHardLink("ignored/nested/file.mkv", downloadFile);
        CreateHardLink("library/file.mkv", downloadFile);

        sut.PopulateFileCounts(ignoredRoot);

        sut.GetHardLinkCount(downloadFile, ignoreRootDir: true).ShouldBe(1);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private static UnixHardLinkFileService CreateSut()
    {
        return new UnixHardLinkFileService(NullLogger<UnixHardLinkFileService>.Instance);
    }

    private string CreateDirectory(string relativePath)
    {
        string path = Path.Combine(_tempRoot, relativePath);
        Directory.CreateDirectory(path);
        return path;
    }

    private string CreateFile(string relativePath)
    {
        string path = Path.Combine(_tempRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "content");
        return path;
    }

    private void CreateHardLink(string relativePath, string targetPath)
    {
        string linkPath = Path.Combine(_tempRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(linkPath)!);
        Syscall.link(targetPath, linkPath).ShouldBe(0);
    }
}
