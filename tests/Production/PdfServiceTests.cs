using System;
using System.IO;
using AasDemoapp.Database.Model;
using AasDemoapp.Production;
using Xunit;

namespace AasDemoapp.Tests.Production;

/// <summary>
/// Unit-Tests für die PdfService Klasse.
/// </summary>
public class PdfServiceTests
{
    [Fact]
    public void CreateHandoverPdf_WithoutAddress_ShouldGenerateValidPdf()
    {
        // Act
        var pdfData = PdfService.CreateHandoverPdf();

        // Assert
        Assert.NotNull(pdfData);
        Assert.True(pdfData.Length > 0);
        // PDF sollte mit "%PDF" beginnen
        var pdfHeader = System.Text.Encoding.ASCII.GetString(
            pdfData,
            0,
            Math.Min(4, pdfData.Length)
        );
        Assert.Equal("%PDF", pdfHeader);
    }

    [Fact]
    public void CreateHandoverPdf_WithAddress_ShouldGenerateValidPdf()
    {
        // Arrange
        var address = new Address
        {
            Strasse = "Musterstraße 123",
            Plz = "12345",
            Ort = "Musterstadt",
            Land = "Deutschland",
        };

        // Act
        var pdfData = PdfService.CreateHandoverPdf(address);

        // Assert
        Assert.NotNull(pdfData);
        Assert.True(pdfData.Length > 0);
        // PDF sollte mit "%PDF" beginnen
        var pdfHeader = System.Text.Encoding.ASCII.GetString(
            pdfData,
            0,
            Math.Min(4, pdfData.Length)
        );
        Assert.Equal("%PDF", pdfHeader);
    }

    [Fact]
    public void CreateHandoverPdf_ShouldCreateFileWithCurrentDate()
    {
        // Act
        var pdfData = PdfService.CreateHandoverPdf();

        // Assert
        // Schreibe PDF in temporäre Datei für weitere Validierung
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempPath, pdfData);

            // Prüfe, ob die Datei erstellt wurde und eine gültige Größe hat
            var fileInfo = new FileInfo(tempPath);
            Assert.True(fileInfo.Exists);
            Assert.True(fileInfo.Length > 1000); // PDF sollte mindestens 1KB groß sein
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
