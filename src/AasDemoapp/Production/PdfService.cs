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
    /// Erzeugt ein PDF-Dokument mit aktuellem Datum, Firmenadresse, Fahrradbild und Bestandteilen
    /// </summary>
    /// <param name="companyAddress">Die Firmenadresse</param>
    /// <param name="producedProduct">Das produzierte Produkt mit Bestandteilen</param>
    /// <returns>PDF als Byte-Array</returns>
    public static byte[] CreateHandoverPdf(
        Address? companyAddress = null,
        ProducedProduct? producedProduct = null
    )
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

        // Layout mit Tabelle: Links Hersteller, Mitte Kundenadresse, rechts Datum/Fahrradbild
        var table = new Table(3).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(30);

        // Linke Spalte: Datum und Adresse
        var leftColumn = new Cell()
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetVerticalAlignment(VerticalAlignment.TOP);

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

            if (
                !string.IsNullOrEmpty(companyAddress.Name)
                || !string.IsNullOrEmpty(companyAddress.Vorname)
            )
                addressText.Add(companyAddress.Name + " " + companyAddress.Vorname + "\n");

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

        // Mittlere Spalte: Kundenadresse
        var middleColumn = new Cell()
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetVerticalAlignment(VerticalAlignment.TOP);

        // Kundenadresse hinzufügen
        var orderAddress = producedProduct?.Order?.Address;
        if (orderAddress != null)
        {
            var customerAddressTitle = new Paragraph("Kunde:")
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginBottom(5);
            middleColumn.Add(customerAddressTitle);

            var customerAddressText = new Paragraph()
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetMarginBottom(15);

            if (
                !string.IsNullOrEmpty(orderAddress.Name)
                || !string.IsNullOrEmpty(orderAddress.Vorname)
            )
                customerAddressText.Add(
                    $"{orderAddress.Vorname} {orderAddress.Name}".Trim() + "\n"
                );

            if (!string.IsNullOrEmpty(orderAddress.Strasse))
                customerAddressText.Add(orderAddress.Strasse + "\n");

            if (!string.IsNullOrEmpty(orderAddress.Plz) || !string.IsNullOrEmpty(orderAddress.Ort))
                customerAddressText.Add($"{orderAddress.Plz} {orderAddress.Ort}".Trim() + "\n");

            if (!string.IsNullOrEmpty(orderAddress.Land))
                customerAddressText.Add(orderAddress.Land);

            middleColumn.Add(customerAddressText);
        }

        table.AddCell(middleColumn);

        // Rechte Spalte: Fahrradbild
        var rightColumn = new Cell()
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetVerticalAlignment(VerticalAlignment.TOP)
            .SetTextAlignment(TextAlignment.CENTER);

        // Aktuelles Datum
        var dateTitle = new Paragraph("Datum:")
            .SetFont(boldFont)
            .SetFontSize(12)
            .SetMarginBottom(5);
        leftColumn.Add(dateTitle);

        var currentDate = new Paragraph(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"))
            .SetFont(normalFont)
            .SetFontSize(12)
            .SetMarginBottom(15);
        leftColumn.Add(currentDate);

        // Fahrradbild hinzufügen
        try
        {
            // Verwende das bike.jpg aus dem AasHandling Ordner
            var possibleImagePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "AasHandling", "bike.jpg"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AasHandling", "bike.jpg"),
                Path.Combine(Directory.GetCurrentDirectory(), "src", "AasHandling", "bike.jpg"),
                // Fallback zu anderen Bildern
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "IMG_4957.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "rahmen.png"),
            };

            byte[]? imageBytes = null;
            foreach (var path in possibleImagePaths)
            {
                if (File.Exists(path))
                {
                    imageBytes = File.ReadAllBytes(path);
                    break;
                }
            }

            if (imageBytes != null && imageBytes.Length > 0)
            {
                var imageTitle = new Paragraph("Produkt:")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetMarginBottom(10)
                    .SetTextAlignment(TextAlignment.CENTER);
                rightColumn.Add(imageTitle);

                var imageData = ImageDataFactory.Create(imageBytes);
                var image = new iText.Layout.Element.Image(imageData)
                    .SetWidth(150)
                    .SetHeight(120)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                rightColumn.Add(image);
            }
            else
            {
                // Kein Bild gefunden - erstelle eine bessere schematische Darstellung
                var imageTitle = new Paragraph("Produkt:")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetMarginBottom(10)
                    .SetTextAlignment(TextAlignment.CENTER);
                rightColumn.Add(imageTitle);

                // ASCII-Art Fahrrad
                var bikeArt =
                    @"      
                            ___
                           (   )    ____
                           |  |    /    
                       ___/  |___/_    __
                      /   \        \_/   \
                     (  O  )--------(  O  )
                      \___/          \___/";

                var bikeAscii = new Paragraph(bikeArt)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.COURIER))
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                rightColumn.Add(bikeAscii);

                var bikeDescription = new Paragraph("Fahrrad")
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

        // QR-Code Sektion für digitalen Produktpass (über die gesamte Breite)
        if (producedProduct != null && !string.IsNullOrWhiteSpace(producedProduct.GlobalAssetId))
        {
            // Trennlinie
            var separator = new Paragraph()
                .SetMarginTop(20)
                .SetMarginBottom(20)
                .SetBorderBottom(
                    new iText.Layout.Borders.SolidBorder(
                        iText.Kernel.Colors.ColorConstants.LIGHT_GRAY,
                        1
                    )
                );
            document.Add(separator);

            // Überschrift
            var dppTitle = new Paragraph("Ihr Digitaler Produktpass")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(15);
            document.Add(dppTitle);

            // Informationstext für den Käufer
            var dppInfoText = new Paragraph(
                "Scannen Sie den QR-Code, um den digitalen Produktpass Ihres Fahrrades aufzurufen. "
                    + "Der digitale Produktpass enthält detaillierte Informationen zu allen verbauten Komponenten, "
                    + "Materialien und technischen Spezifikationen Ihres Fahrrads. "
                    + "Zusätzlich finden Sie dort Wartungshinweise, Herstellerinformationen und können die "
                    + "Lieferkette Ihres Produkts nachvollziehen."
            )
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMarginBottom(20)
                .SetMarginLeft(50)
                .SetMarginRight(50);
            document.Add(dppInfoText);

            try
            {
                var qrCodeBytes = QrCodeService.GenerateQrCodeBytes(
                    producedProduct.GlobalAssetId,
                    8
                );
                if (qrCodeBytes != null)
                {
                    var qrImageData = ImageDataFactory.Create(qrCodeBytes);
                    var qrImage = new iText.Layout.Element.Image(qrImageData)
                        .SetWidth(150)
                        .SetHeight(150)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    // QR-Code mit Rahmen in einer Tabelle für bessere Zentrierung
                    var qrTable = new Table(1)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    var qrCodeCell = new Cell()
                        .Add(qrImage)
                        .SetBorder(
                            new iText.Layout.Borders.SolidBorder(
                                iText.Kernel.Colors.ColorConstants.BLACK,
                                3
                            )
                        )
                        .SetPadding(10)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                        .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.WHITE);

                    qrTable.AddCell(qrCodeCell);
                    document.Add(qrTable);

                    // Label unter dem QR-Code
                    var qrLabel = new Paragraph("Produkt Global Asset ID")
                        .SetFont(normalFont)
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                        .SetMarginTop(10)
                        .SetMarginBottom(20);
                    document.Add(qrLabel);
                }
            }
            catch
            {
                // Fehlerbehandlung für QR-Code-Generierung
                var qrErrorText = new Paragraph("QR-Code konnte nicht erstellt werden")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.RED)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20);
                document.Add(qrErrorText);
            }
        }

        table.AddCell(rightColumn);
        document.Add(table);

        // Bestandteile-Tabelle hinzufügen
        if (producedProduct?.Bestandteile != null && producedProduct.Bestandteile.Any())
        {
            var componentTitle = new Paragraph("Komponenten:")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(componentTitle);
        }
        else
        {
            // Debug-Information hinzufügen wenn keine Bestandteile vorhanden sind
            var debugTitle = new Paragraph("Debug-Information:")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(debugTitle);

            var debugInfo = new Paragraph();
            if (producedProduct == null)
            {
                debugInfo.Add("ProducedProduct ist null\n");
            }
            else if (producedProduct.Bestandteile == null)
            {
                debugInfo.Add("Bestandteile-Liste ist null\n");
            }
            else if (!producedProduct.Bestandteile.Any())
            {
                debugInfo.Add(
                    $"Bestandteile-Liste ist leer (Count: {producedProduct.Bestandteile.Count()})\n"
                );
            }
            else
            {
                var activeComponents = producedProduct
                    .Bestandteile.Where(b => !b.IsDeleted)
                    .Count();
                debugInfo.Add(
                    $"Anzahl Bestandteile: {producedProduct.Bestandteile.Count()}, davon aktiv: {activeComponents}\n"
                );
            }

            debugInfo
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetFontColor(iText.Kernel.Colors.ColorConstants.RED);
            document.Add(debugInfo);
        }

        // Wenn Bestandteile vorhanden sind, erstelle die Tabelle
        if (producedProduct?.Bestandteile != null && producedProduct.Bestandteile.Any())
        {
            // Tabelle für Bestandteile erstellen
            var componentTable = new Table(3)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            // Header-Zeile
            var headerName = new Cell()
                .Add(new Paragraph("Name").SetFont(boldFont).SetFontSize(10))
                .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5);
            componentTable.AddHeaderCell(headerName);

            var headerGlobalAssetId = new Cell()
                .Add(new Paragraph("Global Asset ID").SetFont(boldFont).SetFontSize(10))
                .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5);
            componentTable.AddHeaderCell(headerGlobalAssetId);

            var headerIdShort = new Cell()
                .Add(new Paragraph("QR-Code").SetFont(boldFont).SetFontSize(10))
                .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5);
            componentTable.AddHeaderCell(headerIdShort);

            // Datenzeilen für jeden Bestandteil
            foreach (var bestandteil in producedProduct.Bestandteile.Where(b => !b.IsDeleted))
            {
                // Name
                var nameCell = new Cell()
                    .Add(new Paragraph(bestandteil.Name ?? "").SetFont(normalFont).SetFontSize(9))
                    .SetPadding(5)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                componentTable.AddCell(nameCell);

                // Global Asset ID
                var globalAssetIdCell = new Cell()
                    .Add(
                        new Paragraph(bestandteil.KatalogEintrag?.GlobalAssetId ?? "")
                            .SetFont(normalFont)
                            .SetFontSize(8)
                    )
                    .SetPadding(5)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                componentTable.AddCell(globalAssetIdCell); // QR-Code für Global Asset ID
                var qrCodeCell = new Cell()
                    .SetPadding(1)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetTextAlignment(TextAlignment.CENTER);

                var globalAssetId = bestandteil.KatalogEintrag?.GlobalAssetId;
                if (!string.IsNullOrWhiteSpace(globalAssetId))
                {
                    var qrCodeBytes = QrCodeService.GenerateQrCodeBytes(globalAssetId, 3);
                    if (qrCodeBytes != null)
                    {
                        try
                        {
                            var qrImageData = ImageDataFactory.Create(qrCodeBytes);
                            var qrImage = new iText.Layout.Element.Image(qrImageData)
                                .SetWidth(50)
                                .SetHeight(50)
                                .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                            qrCodeCell.Add(qrImage);
                        }
                        catch
                        {
                            // Fallback auf Text wenn QR-Code nicht erstellt werden kann
                            qrCodeCell.Add(
                                new Paragraph("QR-Fehler")
                                    .SetFont(normalFont)
                                    .SetFontSize(6)
                                    .SetFontColor(iText.Kernel.Colors.ColorConstants.RED)
                            );
                        }
                    }
                    else
                    {
                        // Fallback wenn GlobalAssetId leer ist
                        qrCodeCell.Add(
                            new Paragraph("-")
                                .SetFont(normalFont)
                                .SetFontSize(8)
                                .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                        );
                    }
                }
                else
                {
                    // Fallback wenn GlobalAssetId leer ist
                    qrCodeCell.Add(
                        new Paragraph("-")
                            .SetFont(normalFont)
                            .SetFontSize(8)
                            .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                    );
                }
                componentTable.AddCell(qrCodeCell);
            }

            document.Add(componentTable);
        }

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
