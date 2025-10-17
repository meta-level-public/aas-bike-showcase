using System;
using System.IO;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AasDemoapp.Production;

/// <summary>
/// Service für die Generierung von QR-Codes mit speziellen Anforderungen
/// </summary>
public class QrCodeService
{
    /// <summary>
    /// Generiert einen QR-Code als PNG-Byte-Array mit Rahmen und schwarzer Dreiecksecke
    /// </summary>
    /// <param name="text">Text für den QR-Code</param>
    /// <param name="pixelsPerModule">Pixel pro Modul (Größe)</param>
    /// <returns>PNG-Daten als Byte-Array oder null falls Fehler</returns>
    public static byte[]? GenerateQrCodeBytes(string text, int pixelsPerModule = 10)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        try
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

            // Grundlegenden QR-Code erstellen
            using var qrCode = new PngByteQRCode(qrCodeData);
            var basicQrBytes = qrCode.GetGraphic(pixelsPerModule);

            // QR-Code mit Rahmen und Dreiecksecke erweitern
            return AddFrameAndTriangle(basicQrBytes, pixelsPerModule);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Fügt dem QR-Code einen Rahmen und eine schwarze Dreiecksecke hinzu
    /// Entspricht Requirement 13: Rahmen mit 1 Modul Breite, 4 Module Abstand,
    /// schwarzes Dreieck mit 6-Modul-Beinen in der unteren rechten Ecke
    /// </summary>
    /// <param name="qrCodeBytes">Original QR-Code als PNG-Bytes</param>
    /// <param name="pixelsPerModule">Pixel pro Modul</param>
    /// <returns>Modifizierter QR-Code mit Rahmen und Dreieck</returns>
    private static byte[] AddFrameAndTriangle(byte[] qrCodeBytes, int pixelsPerModule)
    {
        // Original QR-Code laden
        using var originalImage = SixLabors.ImageSharp.Image.Load<Rgba32>(qrCodeBytes);

        // Dimensionen berechnen
        int frameWidth = pixelsPerModule; // Rahmenbreite: 1 Modul
        int quietZone = 4 * pixelsPerModule; // Mindestabstand: 4 Module
        int triangleLeg = 6 * pixelsPerModule; // Dreiecksbein: 6 Module

        // Neue Bildgröße (Original + 2 * (Rahmen + Quiet Zone))
        int margin = frameWidth + quietZone;
        int newWidth = originalImage.Width + 2 * margin;
        int newHeight = originalImage.Height + 2 * margin;

        // Neues Bild erstellen
        using var newImage = new SixLabors.ImageSharp.Image<Rgba32>(newWidth, newHeight);

        // Hintergrund weiß füllen
        newImage.Mutate(ctx => ctx.BackgroundColor(Color.White));

        // Äußeren Rahmen zeichnen (1 Modul breit)
        var frameRect = new RectangleF(
            quietZone,
            quietZone,
            originalImage.Width + 2 * frameWidth,
            originalImage.Height + 2 * frameWidth
        );

        newImage.Mutate(ctx =>
        {
            // Rahmen als gefülltes Rechteck minus inneres Rechteck
            var outerRect = frameRect;
            var innerRect = new RectangleF(
                frameRect.X + frameWidth,
                frameRect.Y + frameWidth,
                frameRect.Width - 2 * frameWidth,
                frameRect.Height - 2 * frameWidth
            );

            // Äußeres Rechteck schwarz füllen
            ctx.Fill(Color.Black, outerRect);
            // Inneres Rechteck weiß füllen (macht den Rahmen)
            ctx.Fill(Color.White, innerRect);
        });

        // Original QR-Code NACH dem Rahmen in die Mitte platzieren
        var qrPosition = new Point(margin, margin);
        newImage.Mutate(ctx => ctx.DrawImage(originalImage, qrPosition, 1.0f));

        // Schwarzes Dreieck in der unteren rechten Ecke des Rahmens - NACH dem QR-Code zeichnen
        var trianglePoints = new PointF[]
        {
            new PointF(frameRect.Right - triangleLeg, frameRect.Bottom), // Unterer linker Punkt
            new PointF(frameRect.Right, frameRect.Bottom), // Unterer rechter Punkt (Ecke)
            new PointF(frameRect.Right, frameRect.Bottom - triangleLeg), // Oberer rechter Punkt
        };

        newImage.Mutate(ctx => ctx.FillPolygon(Color.Black, trianglePoints));

        // Als PNG-Bytes zurückgeben
        using var outputStream = new MemoryStream();
        newImage.Save(outputStream, new PngEncoder());
        return outputStream.ToArray();
    }
}
