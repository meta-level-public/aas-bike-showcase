using System;
using System.IO;
using AasDemoapp.Database.Model;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace AasDemoapp.Production;

public class PdfService
{
    /// <summary>
    /// Erzeugt ein PDF-Dokument mit aktuellem Datum, Firmenadresse und Fahrradbild
    /// </summary>
    /// <param name="companyAddress">Die Firmenadresse</param>
    /// <returns>PDF als Byte-Array</returns>
    public static byte[] CreateHandoverPdf(Address? companyAddress = null)
    {
        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        // Schriftarten definieren
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // Titel
        var title = new Paragraph("Übergabe-Dokumentation")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20);
        document.Add(title);

        // Layout mit Tabelle: Links Datum/Adresse, rechts Fahrradbild
        var table = new Table(2).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(30);

        // Linke Spalte: Datum und Adresse
        var leftColumn = new Cell()
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetVerticalAlignment(VerticalAlignment.TOP);

        // Aktuelles Datum
        var dateTitle = new Paragraph("Datum:")
            .SetFont(boldFont)
            .SetFontSize(12)
            .SetMarginBottom(5);
        leftColumn.Add(dateTitle);

        var currentDate = new Paragraph(DateTime.Now.ToString("dd.MM.yyyy"))
            .SetFont(normalFont)
            .SetFontSize(12)
            .SetMarginBottom(15);
        leftColumn.Add(currentDate);

        // Firmenadresse
        if (companyAddress != null)
        {
            var addressTitle = new Paragraph("Hersteller:")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginBottom(5);
            leftColumn.Add(addressTitle);

            var addressText = new Paragraph()
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetMarginBottom(15);

            if (!string.IsNullOrEmpty(companyAddress.Strasse))
                addressText.Add(companyAddress.Strasse + "\n");

            if (
                !string.IsNullOrEmpty(companyAddress.Plz)
                || !string.IsNullOrEmpty(companyAddress.Ort)
            )
                addressText.Add($"{companyAddress.Plz} {companyAddress.Ort}\n");

            if (!string.IsNullOrEmpty(companyAddress.Land))
                addressText.Add(companyAddress.Land);

            leftColumn.Add(addressText);
        }

        table.AddCell(leftColumn);

        // Rechte Spalte: Fahrradbild
        var rightColumn = new Cell()
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetVerticalAlignment(VerticalAlignment.TOP)
            .SetTextAlignment(TextAlignment.CENTER);

        // Fahrradbild hinzufügen
        try
        {
            // Versuche verschiedene Pfade für das Bild
            var possibleImagePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "rahmen.png"),
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "wwwroot",
                    "images",
                    "rahmen.png"
                ),
                Path.Combine(Directory.GetCurrentDirectory(), "images", "rahmen.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "src", "images", "rahmen.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "rahmen.png"),
                // Fallback zu IMG_4957.png
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "IMG_4957.png"),
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "wwwroot",
                    "images",
                    "IMG_4957.png"
                ),
                Path.Combine(Directory.GetCurrentDirectory(), "images", "IMG_4957.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "src", "images", "IMG_4957.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "IMG_4957.png"),
            };

            string? workingImagePath = null;
            foreach (var path in possibleImagePaths)
            {
                if (File.Exists(path))
                {
                    workingImagePath = path;
                    break;
                }
            }

            if (workingImagePath != null)
            {
                var imageTitle = new Paragraph("Produkt:")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetMarginBottom(10)
                    .SetTextAlignment(TextAlignment.CENTER);
                rightColumn.Add(imageTitle);

                var imageData = ImageDataFactory.Create(workingImagePath);
                var image = new Image(imageData)
                    .SetWidth(150)
                    .SetHeight(120)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                rightColumn.Add(image);
            }
            else
            {
                // Kein Bild gefunden - erstelle eine einfache schematische Darstellung mit Text
                var imageTitle = new Paragraph("Produkt:")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetMarginBottom(10)
                    .SetTextAlignment(TextAlignment.CENTER);
                rightColumn.Add(imageTitle);

                // Einfache schematische Darstellung als Text
                var bikeSchematic = new Paragraph("🚲")
                    .SetFont(normalFont)
                    .SetFontSize(48)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                rightColumn.Add(bikeSchematic);

                var bikeDescription = new Paragraph("Fahrrad\n(Schematische Darstellung)")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY);
                rightColumn.Add(bikeDescription);
            }
        }
        catch (Exception ex)
        {
            // Falls das Bild nicht geladen werden kann, füge einen Platzhaltertext hinzu
            var imageErrorText = new Paragraph(
                "Produktbild konnte nicht geladen werden: " + ex.Message
            )
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetFontColor(iText.Kernel.Colors.ColorConstants.RED)
                .SetTextAlignment(TextAlignment.CENTER);
            rightColumn.Add(imageErrorText);
        }

        table.AddCell(rightColumn);
        document.Add(table);

        // Zusätzliche Informationen
        var additionalInfo = new Paragraph("Diese Dokumentation wurde automatisch generiert.")
            .SetFont(normalFont)
            .SetFontSize(8)
            .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginTop(30);
        document.Add(additionalInfo);

        document.Close();
        return stream.ToArray();
    }
}
