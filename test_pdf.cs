using System;
using System.IO;
using AasDemoapp.Database.Model;
using AasDemoapp.Production;

// Teste die PDF-Generierung
Console.WriteLine("🧪 Testing PDF Generation...");

try
{
    var address = new Address
    {
        Name = "OI4 Nextbike GmbH",
        Strasse = "Musterstraße 123",
        Plz = "12345",
        Ort = "Musterstadt",
        Land = "Deutschland",
    };

    var pdfData = PdfService.CreateHandoverPdf(address);

    Console.WriteLine($"✅ PDF generated successfully!");
    Console.WriteLine($"📊 File size: {pdfData.Length} bytes");
    Console.WriteLine($"📄 PDF Header: {System.Text.Encoding.ASCII.GetString(pdfData, 0, 4)}");

    // Save to desktop for verification
    var fileName = $"test_pdf_{DateTime.Now:yyyyMMddHHmmss}.pdf";
    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    var filePath = Path.Combine(desktopPath, fileName);

    File.WriteAllBytes(filePath, pdfData);
    Console.WriteLine($"💾 PDF saved to: {filePath}");

    Console.WriteLine("🎉 Test completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
