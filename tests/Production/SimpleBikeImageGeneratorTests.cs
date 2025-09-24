using System;
using System.IO;
using AasDemoapp.Production;
using Xunit;

namespace AasDemoapp.Tests.Production;

public class SimpleBikeImageGeneratorTests
{
    [Fact]
    public void CreateSimpleBikePng_ShouldCreateValidImageFile()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        var outputPath = Path.ChangeExtension(tempPath, ".png");

        try
        {
            // Act
            SimpleBikeImageGenerator.CreateSimpleBikePng(outputPath, 400, 250);

            // Assert
            Assert.True(File.Exists(outputPath), "PNG file should be created");

            var fileInfo = new FileInfo(outputPath);
            Assert.True(fileInfo.Length > 0, "PNG file should not be empty");

            // Verify PNG header
            var headerBytes = new byte[8];
            using (var fileStream = File.OpenRead(outputPath))
            {
                fileStream.Read(headerBytes, 0, 8);
            }

            // PNG header should be: 89 50 4E 47 0D 0A 1A 0A
            Assert.Equal(0x89, headerBytes[0]);
            Assert.Equal(0x50, headerBytes[1]); // 'P'
            Assert.Equal(0x4E, headerBytes[2]); // 'N'
            Assert.Equal(0x47, headerBytes[3]); // 'G'
        }
        finally
        {
            // Cleanup
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void CreateSimpleBikePng_WithInvalidPath_ShouldThrowException()
    {
        // Arrange
        var invalidPath = "/invalid/path/that/does/not/exist/test.png";

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() =>
            SimpleBikeImageGenerator.CreateSimpleBikePng(invalidPath, 400, 250)
        );
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(400, 250)]
    [InlineData(800, 600)]
    public void CreateSimpleBikePng_WithDifferentSizes_ShouldCreateValidImages(
        int width,
        int height
    )
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        var outputPath = Path.ChangeExtension(tempPath, ".png");

        try
        {
            // Act
            SimpleBikeImageGenerator.CreateSimpleBikePng(outputPath, width, height);

            // Assert
            Assert.True(
                File.Exists(outputPath),
                $"PNG file should be created for size {width}x{height}"
            );

            var fileInfo = new FileInfo(outputPath);
            Assert.True(
                fileInfo.Length > 0,
                $"PNG file should not be empty for size {width}x{height}"
            );
        }
        finally
        {
            // Cleanup
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void CreateSimpleBikePng_ShouldCreateDirectoryIfNotExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var outputPath = Path.Combine(tempDir, "test_bike.png");

        try
        {
            // Act
            SimpleBikeImageGenerator.CreateSimpleBikePng(outputPath, 400, 250);

            // Assert
            Assert.True(
                Directory.Exists(tempDir),
                "Directory should be created if it doesn't exist"
            );
            Assert.True(File.Exists(outputPath), "PNG file should be created");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
