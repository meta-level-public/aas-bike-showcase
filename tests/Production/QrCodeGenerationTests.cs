using AasDemoapp.Production;
using Xunit;

namespace AasDemoapp.Tests.Production;

public class QrCodeGenerationTests
{
    [Fact]
    public void GenerateQrCodeBytes_WithValidText_ShouldReturnValidBytes()
    {
        // Arrange
        var testText = "https://example.com/globalAssetId/test123";

        // Act
        var result = PdfService.GenerateQrCodeBytes(testText, 3);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.True(result.Length > 100); // PNG should be reasonably sized

        // Check PNG header
        Assert.Equal(0x89, result[0]); // PNG signature
        Assert.Equal(0x50, result[1]); // 'P'
        Assert.Equal(0x4E, result[2]); // 'N'
        Assert.Equal(0x47, result[3]); // 'G'
    }

    [Fact]
    public void GenerateQrCodeBytes_WithEmptyText_ShouldReturnNull()
    {
        // Act
        var result = PdfService.GenerateQrCodeBytes("", 3);

        // Assert
        Assert.Null(result);
    }
}
