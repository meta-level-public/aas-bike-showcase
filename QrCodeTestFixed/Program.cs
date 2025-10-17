using System;
using System.IO;
using AasDemoapp.Production;

// Test-Script für den QR-Code mit Rahmen und Dreieck
Console.WriteLine("Testing QR Code with frame and triangle (fixed version)...");

// Test-Text für den QR-Code
string testText = "https://example.com/test-asset-id-456";

// QR-Code generieren mit der neuen QrCodeService
var qrCodeBytes = QrCodeService.GenerateQrCodeBytes(testText, 5);

if (qrCodeBytes != null)
{
    // QR-Code in eine Datei speichern
    string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "test_qr_code_fixed.png");
    File.WriteAllBytes(outputPath, qrCodeBytes);

    Console.WriteLine($"QR-Code successfully generated and saved to: {outputPath}");
    Console.WriteLine($"Size: {qrCodeBytes.Length} bytes");

    // Öffne das Bild im Standard-Bildviewer
    try
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "open";
        process.StartInfo.Arguments = outputPath;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        Console.WriteLine("Image opened in default viewer");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not open image: {ex.Message}");
    }
}
else
{
    Console.WriteLine("Failed to generate QR code!");
}
