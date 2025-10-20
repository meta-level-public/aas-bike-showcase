using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling;
using AasDemoapp.AasHandling.SubmodelCreators;
using AasDemoapp.Database.Model;
using AasDemoapp.Import;
using AasDemoapp.Settings;
using AasDemoapp.Utils;
using AasDemoapp.Utils.Shells;
using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using Namotion.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AasDemoapp.Production;

public record HandoverDocumentationResult(
    Submodel Submodel,
    ProvidedFile? PdfFile,
    ProvidedFile? PdfPreviewImageFile
);

public class InstanceAasCreator
{
    private static Microsoft.Extensions.Logging.ILogger<InstanceAasCreator>? _logger;

    public static void InitializeLogger(
        Microsoft.Extensions.Logging.ILogger<InstanceAasCreator> logger
    )
    {
        _logger = logger;
    }

    /// <summary>
    /// Erzeugt ein Vorschaubild (PNG) der ersten Seite eines PDF Dokuments.
    /// Nutzt Docnet.Core (MIT) für das Rendering und ImageSharp (Apache-2.0) für PNG-Encoding.
    /// Fallback/Fehlschlag wird geloggt und gibt null zurück. Läuft plattformunabhängig (Windows/macOS/Linux)
    /// und ist damit docker-/container-tauglich ohne native Abhängigkeiten.
    /// </summary>
    /// <param name="pdfBytes">PDF als Bytearray</param>
    /// <param name="originalPdfFileName">Originaler PDF Dateiname (zur Ableitung des Preview-Namens)</param>
    /// <returns>ProvidedFile für die Preview PNG oder null</returns>
    private static ProvidedFile? TryCreatePdfPreview(byte[] pdfBytes, string originalPdfFileName)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
            return null;

        try
        {
            // PageDimensions dürfen nicht 0 sein (Docnet wirft sonst ArgumentException "value can't be less or equal to zero (dimOne)").
            // Wir rendern die erste Seite mit einer maximalen Zielgröße (A4 Hochformat grob ~1.414 Verhältnis) und skalieren ggf. später erneut.
            const int targetRenderWidth = 1024; // ausreichende Preview-Qualität
            const int targetRenderHeight = 1448; // 1024 * 1.414 (gerundet)
            using var docReader = DocLib.Instance.GetDocReader(
                pdfBytes,
                new PageDimensions(targetRenderWidth, targetRenderHeight)
            );
            if (docReader.GetPageCount() == 0)
                return null;

            using var pageReader = docReader.GetPageReader(0);
            var rawBytes = pageReader.GetImage();
            var pageWidth = pageReader.GetPageWidth();
            var pageHeight = pageReader.GetPageHeight();

            // Docnet liefert BGRA ( laut Doku ), wir mappen nach ImageSharp PixelFormat Rgba32
            // Rohdatenlänge prüfen (w * h * 4)
            if (rawBytes == null || rawBytes.Length != pageWidth * pageHeight * 4)
            {
                _logger?.LogWarning(
                    "PDF Preview: Unerwartete Pixeldaten-Länge (expected {Expected}, actual {Actual})",
                    pageWidth * pageHeight * 4,
                    rawBytes?.Length
                );
                return null;
            }

            // BGRA -> RGBA konvertieren (Inplace Kopie in neues Array)
            for (var i = 0; i < rawBytes.Length; i += 4)
            {
                var b = rawBytes[i];
                var r = rawBytes[i + 2];
                rawBytes[i] = r; // R an Position 0
                rawBytes[i + 2] = b; // B an Position 2
            }

            // Image anlegen
            using var image = Image.LoadPixelData<Rgba32>(rawBytes, pageWidth, pageHeight);

            // Optional skalieren, um große PDFs kleiner zu machen (Max 1024 Breite)
            const int maxWidth = 1024;
            if (pageWidth > maxWidth)
            {
                var ratio = (double)maxWidth / pageWidth;
                var targetWidth = maxWidth;
                var targetHeight = (int)Math.Round(pageHeight * ratio);
                image.Mutate(ctx => ctx.Resize(targetWidth, targetHeight));
            }

            var previewFileName = Path.ChangeExtension(originalPdfFileName, null);
            if (string.IsNullOrWhiteSpace(previewFileName))
                previewFileName = "pdf_preview";
            previewFileName += "_page1.png";

            var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            ms.Position = 0;

            return new ProvidedFile
            {
                Stream = ms,
                Filename = previewFileName,
                Type = ProvidedFileType.Thumbnail,
                ContentType = "image/png",
            };
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "PDF Preview konnte nicht erstellt werden");
            return null;
        }
    }

    /// <summary>
    /// Lädt die Bilddatei (bike.jpg) als ProvidedFile für den Thumbnail-Upload.
    /// Sucht zuerst im Output-Verzeichnis (AppContext.BaseDirectory/AasHandling),
    /// danach relativ zum Projekt (./src/AasDemoapp/AasHandling/bike.jpg) für Entwicklungsumgebungen.
    /// </summary>
    /// <returns>ProvidedFile oder null falls nicht gefunden oder Fehler.</returns>
    private static ProvidedFile? TryLoadThumbnailFile()
    {
        try
        {
            var thumbnailSourcePath = Path.Combine(
                AppContext.BaseDirectory,
                "AasHandling",
                "bike.jpg"
            );
            if (!System.IO.File.Exists(thumbnailSourcePath))
            {
                // Fallback: Entwicklungszeit - relativ zum Projektroot (z.B. beim watch run)
                var devRoot = Directory.GetCurrentDirectory();
                var possible = Path.Combine(devRoot, "src", "AasHandling", "bike.jpg");
                if (System.IO.File.Exists(possible))
                {
                    thumbnailSourcePath = possible;
                }
            }

            if (System.IO.File.Exists(thumbnailSourcePath))
            {
                return new ProvidedFile
                {
                    Stream = System.IO.File.OpenRead(thumbnailSourcePath),
                    Filename = "thumbnail.jpg", // bleibt konsistent mit AssetInformation Resource
                    Type = ProvidedFileType.Added,
                    ContentType = "image/jpeg",
                };
            }
            _logger?.LogWarning("Thumbnail-Bild nicht gefunden: {Path}", thumbnailSourcePath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Thumbnail konnte nicht geladen werden");
        }
        return null;
    }

    public static async Task<AssetAdministrationShell> CreateBikeInstanceAas(
        ProducedProduct producedProduct,
        IImportService importService,
        ISettingService settingsService
    )
    {
        var assetInformation = new AssetInformation(
            AssetKind.Instance,
            producedProduct.GlobalAssetId,
            null,
            producedProduct.GlobalAssetId,
            new Resource("thumbnail.jpg", "image/jpeg")
        );

        assetInformation.AssetType = producedProduct.ConfiguredProduct.GlobalAssetId; // Set the asset type to the configured product's global asset ID
        var aas = new AssetAdministrationShell(
            producedProduct.AasId,
            assetInformation,
            null,
            null,
            producedProduct.ConfiguredProduct.Name
        );

        // Lade Firmenadresse aus den Einstellungen
        var currentAddressSetting =
            settingsService.GetSetting(SettingTypes.CompanyAddress)?.Value ?? "";

        Address? companyAddress = null;
        if (!string.IsNullOrEmpty(currentAddressSetting))
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                companyAddress = System.Text.Json.JsonSerializer.Deserialize<Address>(
                    currentAddressSetting,
                    options
                );
            }
            catch
            {
                // Falls die Deserialisierung fehlschlägt, verwende null
                companyAddress = null;
            }
        }

        var nameplate = CreateNameplateSubmodel(companyAddress, producedProduct);
        aas.Submodels =
        [
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, nameplate.Id)]
            ),
        ];

        var handoverResult = CreateHandoverDocumentation(companyAddress, producedProduct);
        aas.Submodels.Add(
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, handoverResult.Submodel.Id)]
            )
        );

        var weight = 5.4; // default gewicht in kg

        var technicalData = CreateTechnicalData(weight);
        aas.Submodels.Add(
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, technicalData.Id)]
            )
        );

        var hierarchicalStructures = CreateHierarchicalStructuresSubmodel(producedProduct);
        aas.Submodels.Add(
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, hierarchicalStructures.Id)]
            )
        );

        var productCarbonFootprint = CreateProductCarbonFootprintSubmodel(
            producedProduct,
            settingsService,
            weight
        );
        aas.Submodels.Add(
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, productCarbonFootprint.Id)]
            )
        );

        var files = new List<ProvidedFile>();
        if (handoverResult.PdfFile != null)
            files.Add(handoverResult.PdfFile);
        if (handoverResult.PdfPreviewImageFile != null)
            files.Add(handoverResult.PdfPreviewImageFile);
        var ceFile = TryLoadCeFile();
        if (ceFile != null)
        {
            files.Add(ceFile); // CE Kennzeichen laden (ausgelagerte Methode)
        }

        // Thumbnail laden (ausgelagerte Methode)
        var thumbnailFile = TryLoadThumbnailFile();
        if (thumbnailFile != null)
        {
            files.Add(thumbnailFile);
        }

        await SaveAasToRepositories(
            aas,
            [
                nameplate,
                handoverResult.Submodel,
                technicalData,
                hierarchicalStructures,
                productCarbonFootprint,
            ],
            importService,
            settingsService,
            files
        );

        return aas;
    }

    private static ProvidedFile? TryLoadCeFile()
    {
        try
        {
            var ceSourcePath = Path.Combine(AppContext.BaseDirectory, "AasHandling", "ce.png");
            if (!System.IO.File.Exists(ceSourcePath))
            {
                // Fallback: Entwicklungszeit - relativ zum Projektroot (z.B. beim watch run)
                var devRoot = Directory.GetCurrentDirectory();
                var possible = Path.Combine(devRoot, "src", "AasHandling", "ce.png");
                if (System.IO.File.Exists(possible))
                {
                    ceSourcePath = possible;
                }
            }

            if (System.IO.File.Exists(ceSourcePath))
            {
                return new ProvidedFile
                {
                    Stream = System.IO.File.OpenRead(ceSourcePath),
                    Filename = "ce.png",
                    Type = ProvidedFileType.Added,
                    ContentType = "image/png",
                };
            }
            _logger?.LogWarning("CE Logo nicht gefunden: {Path}", ceSourcePath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CE Logo konnte nicht geladen werden");
        }
        return null;
    }

    private static async Task SaveAasToRepositories(
        AssetAdministrationShell aas,
        List<ISubmodel> submodels,
        IImportService importService,
        ISettingService settingsService,
        List<ProvidedFile>? providedFiles = null
    )
    {
        var securitySetting = settingsService.GetSecuritySetting(
            SettingTypes.InfrastructureSecurity
        );
        var aasRepositoryUrl =
            settingsService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "";
        var submodelRepositoryUrl =
            settingsService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.Value ?? "";
        var aasRegistryUrl = settingsService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "";
        var submodelRegistryUrl =
            settingsService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.Value ?? "";

        var env = new AasCore.Aas3_0.Environment
        {
            AssetAdministrationShells = [aas],
            Submodels = submodels,
        };
        var plainJson = AasCore.Aas3_0.Jsonization.Serialize.ToJsonObject(env).ToJsonString();

        await SaveShellSaver.SaveSingle(
            new AasUrls
            {
                AasRepositoryUrl = aasRepositoryUrl,
                SubmodelRepositoryUrl = submodelRepositoryUrl,
                AasRegistryUrl = aasRegistryUrl,
                SubmodelRegistryUrl = submodelRegistryUrl,
            },
            securitySetting,
            plainJson,
            providedFiles ?? [],
            default
        );
    }

    private static HandoverDocumentationResult CreateHandoverDocumentation(
        Address? companyAddress = null,
        ProducedProduct? producedProduct = null
    )
    {
        var handoverdoc = HandoverDocumentationCreator.CreateFromJson();
        handoverdoc.Description =
        [
            new LangStringTextType("de", "Handover documentation for the configured product"),
        ];

        // Generiere PDF-Dokument mit Bestandteilen
        var pdfData = PdfService.CreateHandoverPdf(companyAddress, producedProduct);
        var pdfFileName = $"handover_documentation_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

        // Erstelle ProvidedFile für das PDF
        var pdfStream = new MemoryStream(pdfData);
        var providedFile = new ProvidedFile
        {
            Stream = pdfStream,
            Filename = pdfFileName,
            Type = ProvidedFileType.Added,
            ContentType = "application/pdf",
        };

        // Preview (erste Seite) erzeugen
        ProvidedFile? previewFile = null;
        try
        {
            previewFile = TryCreatePdfPreview(pdfData, pdfFileName);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "PDF Preview konnte nicht erzeugt werden");
        }

        // Finde das erste Document Element im Submodel und aktualisiere es
        if (handoverdoc.SubmodelElements != null)
        {
            var documentCollection = handoverdoc
                .SubmodelElements.OfType<SubmodelElementCollection>()
                .FirstOrDefault(smc => smc.IdShort?.StartsWith("Document") == true);

            if (documentCollection != null && documentCollection.Value != null)
            {
                documentCollection.IdShort = "Uebergabedokumentation";

                // das File befindet sich in der SMC mit der idShort DocumentVersion__*
                var documentVersionCollection = documentCollection
                    .Value.OfType<SubmodelElementCollection>()
                    .FirstOrDefault(smc => smc.IdShort?.StartsWith("DocumentVersion") == true);

                if (documentVersionCollection?.Value != null)
                {
                    // Finde das erste DigitalFile Element in der DocumentVersion SMC
                    var digitalFileElement = documentVersionCollection
                        .Value.OfType<AasCore.Aas3_0.File>()
                        .FirstOrDefault(f => f.IdShort?.StartsWith("DigitalFile") == true);

                    if (digitalFileElement != null)
                    {
                        // Aktualisiere das DigitalFile Element mit dem generierten PDF
                        digitalFileElement.ContentType = "application/pdf";
                        digitalFileElement.Value = pdfFileName; // Verwende den Dateinamen als Referenz
                    }
                    else
                    {
                        // Debug: Logge alle verfügbaren Elemente in DocumentVersion
                        _logger?.LogDebug(
                            "DigitalFile nicht gefunden in DocumentVersion. Elemente werden gelistet"
                        );
                        foreach (var element in documentVersionCollection.Value)
                        {
                            _logger?.LogDebug(
                                "Element {Type} mit IdShort {IdShort}",
                                element.GetType().Name,
                                element.IdShort
                            );
                        }
                    }

                    // Vorschaubild einfügen, falls verfügbar
                    if (previewFile != null)
                    {
                        var previewFileElement = documentVersionCollection
                            .Value.OfType<AasCore.Aas3_0.File>()
                            .FirstOrDefault(f => f.IdShort?.StartsWith("PreviewFile") == true);
                        if (previewFileElement != null && previewFile != null)
                        {
                            previewFileElement.ContentType = "image/png";
                            previewFileElement.Value = previewFile.Filename; // Verwende den Dateinamen als Referenz
                        }
                    }
                }
                else
                {
                    // Debug: Logge alle verfügbaren Elemente in Document
                    _logger?.LogDebug(
                        "DocumentVersion nicht gefunden. Elemente in Document werden gelistet"
                    );
                    foreach (var element in documentCollection.Value)
                    {
                        _logger?.LogDebug(
                            "Element {Type} mit IdShort {IdShort}",
                            element.GetType().Name,
                            element.IdShort
                        );
                    }
                }
            }
            else
            {
                // Debug: Logge alle verfügbaren Top-Level Elemente
                _logger?.LogDebug("Document nicht gefunden. Top-Level Elemente werden gelistet");
                foreach (var element in handoverdoc.SubmodelElements)
                {
                    _logger?.LogDebug(
                        "Element {Type} mit IdShort {IdShort}",
                        element.GetType().Name,
                        element.IdShort
                    );
                }
            }
        }

        return new HandoverDocumentationResult(handoverdoc, providedFile, previewFile);
    }

    private static Submodel CreateNameplateSubmodel(
        Address? companyAddress = null,
        ProducedProduct? producedProduct = null
    )
    {
        var nameplate = NameplateCreator.CreateFromJson();
        PropertyValueChanger.SetPropertyValueByPath(
            "ManufacturerName",
            companyAddress?.Name ?? string.Empty,
            nameplate
        );
        PropertyValueChanger.SetPropertyValueByPath(
            "URIOfTheProduct",
            "https://app.aas-bike.showcasehub.de/",
            nameplate
        );
        PropertyValueChanger.SetPropertyValueByPath(
            "ManufacturerProductDesignation",
            "Kundenkonfiguriertes Fahrrad",
            nameplate
        );
        PropertyValueChanger.SetPropertyValueByPath(
            "ManufacturerProductRoot",
            "Fahrrad",
            nameplate
        );
        PropertyValueChanger.SetPropertyValueByPath(
            "ManufacturerProductType",
            "NextBike",
            nameplate
        );
        PropertyValueChanger.SetPropertyValueByPath(
            "SerialNumber",
            "NB-" + producedProduct?.Id.ToString(),
            nameplate
        );
        PropertyValueChanger.SetPropertyValueByPath(
            "SerialNumber",
            "NB-Type-" + producedProduct?.ConfiguredProductId.ToString(),
            nameplate
        );
        PropertyValueChanger.SetPropertyValueByPath(
            "YearOfConstruction",
            DateTime.Now.Year.ToString(),
            nameplate
        );
        PropertyValueChanger.SetPropertyValueByPath(
            "DateOfManufacture",
            DateTime.Now.ToString("O"),
            nameplate
        );
        PropertyValueChanger.SetPropertyValueByPath(
            "CountryOfOrigin",
            companyAddress?.Land ?? string.Empty,
            nameplate
        );

        // adresse

        var contactInfo = ContactInformationCreator.CreateFromJson(
            (companyAddress?.Name == null && companyAddress?.Vorname == null)
                ? string.Empty
                : (companyAddress?.Name ?? string.Empty)
                    + " "
                    + (companyAddress?.Vorname ?? string.Empty),
            companyAddress?.Strasse ?? string.Empty,
            companyAddress?.Ort ?? string.Empty,
            companyAddress?.Plz ?? string.Empty,
            companyAddress?.Land ?? string.Empty
        );

        var contactInfoSmc = nameplate
            .SubmodelElements?.OfType<SubmodelElementCollection>()
            .FirstOrDefault(smc => smc.IdShort == "AddressInformation");
        if (contactInfoSmc != null)
        {
            if (contactInfoSmc.Value == null)
            {
                contactInfoSmc.Value = [];
            }
            contactInfoSmc.Value = contactInfo.Value;
        }

        return nameplate;
    }

    private static Submodel CreateTechnicalData(double weight)
    {
        var technicalData = TechnicalDataCreator.CreateFromJson();

        var collectionTechnicalData = new SubmodelElementCollection();
        collectionTechnicalData.Value = [];

        var weightProp = new Property(DataTypeDefXsd.Double);
        weightProp.IdShort = "Weight";
        weightProp.Value = weight.ToString("F2"); // 2 Dezimalstellen

        weightProp.SemanticId = new Reference(
            ReferenceTypes.ExternalReference,
            [new Key(KeyTypes.GlobalReference, "0173-1#02-AAJ633#004")]
        );

        collectionTechnicalData.Value.Add(weightProp);

        var parentSmc = technicalData
            .SubmodelElements?.OfType<SubmodelElementList>()
            .FirstOrDefault(smc => smc.IdShort == "TechnicalPropertyAreas");

        parentSmc?.Value?.Add(collectionTechnicalData);

        return technicalData;
    }

    private static Submodel CreateHierarchicalStructuresSubmodel(ProducedProduct producedProduct)
    {
        var hierarchicalStructures = HierarchicalStructuresCreator.CreateFromJson();
        // entryNode ist das konfigurierte Produkt
        var entryNode = new Entity(EntityType.SelfManagedEntity);
        entryNode.SemanticId = new Reference(
            ReferenceTypes.ExternalReference,
            [
                new Key(
                    KeyTypes.GlobalReference,
                    "https://admin-shell.io/idta/HierarchicalStructures/EntryNode/1/0"
                ),
            ]
        );
        entryNode.IdShort = producedProduct.ConfiguredProduct.Name.Replace(" ", "_");
        entryNode.GlobalAssetId = producedProduct.GlobalAssetId;
        entryNode.Statements = [];
        var first = new Reference(
            ReferenceTypes.ModelReference,
            [
                new Key(KeyTypes.Submodel, hierarchicalStructures.Id),
                new Key(KeyTypes.Entity, entryNode.IdShort),
            ]
        );
        // die elemente einfügen, die in der Konfiguration des Produkts enthalten sind
        foreach (var part in producedProduct.Bestandteile)
        {
            var partNode = new Entity(EntityType.SelfManagedEntity);
            partNode.IdShort = part.Name.Replace(" ", "_");
            partNode.SemanticId = new Reference(
                ReferenceTypes.ExternalReference,
                [
                    new Key(
                        KeyTypes.GlobalReference,
                        "https://admin-shell.io/idta/HierarchicalStructures/Node/1/0"
                    ),
                ]
            );
            partNode.Statements = [];
            partNode.GlobalAssetId = part.KatalogEintrag.GlobalAssetId;

            var second = new Reference(
                ReferenceTypes.ModelReference,
                [
                    new Key(KeyTypes.Submodel, hierarchicalStructures.Id),
                    new Key(KeyTypes.Entity, entryNode.IdShort),
                    new Key(KeyTypes.Entity, partNode.IdShort),
                ]
            );

            var hasPartRelation = new RelationshipElement(first, second);
            hasPartRelation.SemanticId = new Reference(
                ReferenceTypes.ExternalReference,
                [
                    new Key(
                        KeyTypes.GlobalReference,
                        "https://admin-shell.io/idta/HierarchicalStructures/HasPart/1/0"
                    ),
                ]
            );
            hasPartRelation.IdShort = "HasPart";

            var bulkCount = new Property(DataTypeDefXsd.Integer);
            bulkCount.SemanticId = new Reference(
                ReferenceTypes.ExternalReference,
                [
                    new Key(
                        KeyTypes.GlobalReference,
                        "https://admin-shell.io/idta/HierarchicalStructures/BulkCount/1/0"
                    ),
                ]
            );
            bulkCount.IdShort = "BulkCount";
            bulkCount.Value = part.Amount.ToString();

            partNode.Statements.Add(bulkCount);
            entryNode.Statements.Add(partNode);
            entryNode.Statements.Add(hasPartRelation);
        }

        hierarchicalStructures.SubmodelElements = [entryNode];

        return hierarchicalStructures;
    }

    private static Submodel CreateProductCarbonFootprintSubmodel(
        ProducedProduct producedProduct,
        ISettingService settingsService,
        double weight
    )
    {
        var pcf = PCFCreator.CreateFromJsonPrefilled();
        var productFootprints = (SubmodelElementList?)
            pcf.SubmodelElements?.Find(submodel => submodel.IdShort == "ProductCarbonFootprints");
        // add pcf value, publication date and address for phases A1 - A3
        var pcfComponent = (SubmodelElementCollection?)productFootprints?.Value?.FirstOrDefault();
        var currentAddressSetting =
            settingsService.GetSetting(SettingTypes.CompanyAddress)?.Value ?? "";

        Address? addressCompany = null;
        if (!string.IsNullOrEmpty(currentAddressSetting))
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                addressCompany = System.Text.Json.JsonSerializer.Deserialize<Address>(
                    currentAddressSetting,
                    options
                );
            }
            catch (Exception)
            {
                // if deserialization fails, we just leave the address as null
            }
        }

        if (pcfComponent != null)
        {
            var pcfValue = (producedProduct.PCFValue * 0.2);
            if (pcfValue == 0)
            {
                // zufälligen Wert zwischen 0 und 5 erzeugen mit 3 nachkommastellen
                var rand = new Random();
                pcfValue = Math.Round(rand.NextDouble() * 5, 3);
            }
            CompletePCFData(pcfComponent, pcfValue.ToString(), addressCompany);
        }

        // add pcf value, publication date and address for phase A4, if applicable (else ignore)
        try
        {
            // Transport PCF ist 120g pro Tonnenkilometer
            // Distanz zwischen producedProduct.Order.Address und companyAddress berechnen
            var transportDistance =
                DistanceCalculator.CalculateDistanceKm(
                    addressCompany,
                    producedProduct.Order.Address
                ) ?? 100; // Fallback auf 100km wenn keine Koordinaten verfügbar

            var pcfComponentTransport = (SubmodelElementCollection?)productFootprints?.Value?[1];
            if (pcfComponentTransport != null)
            {
                // Transport PCF Berechnung: 120g CO2 pro Tonnenkilometer
                // Annahme: Fahrrad wiegt ca. 15kg = 0.015 Tonnen
                var weightInTonnes = weight / 1000;
                var transportPcfValue = transportDistance * weightInTonnes * 0.12; // 120g = 0.12kg CO2 pro Tonnenkilometer

                CompletePCFData(
                    pcfComponentTransport,
                    transportPcfValue.ToString("F3"), // 3 Dezimalstellen
                    producedProduct.Order.Address
                );
            }
        }
        catch (IndexOutOfRangeException) { } // transport pcf not applilcable (yet)
        return pcf;
    }

    private static SubmodelElementCollection CompletePCFData(
        SubmodelElementCollection pcfComponent,
        String pcfValue,
        Address? address
    )
    {
        var pcfElem = (Property?)
            pcfComponent.Value?.Find(property => property.IdShort == "PcfCO2eq");
        if (pcfElem != null)
            pcfElem.Value = pcfValue;
        var publicationDate = (Property?)
            pcfComponent.Value?.Find(property => property.IdShort == "PublicationDate");
        if (publicationDate != null)
            // iso string schreiben
            publicationDate.Value = DateTime.Now.ToString("O");
        var goodsHandoverAddress = (SubmodelElementCollection?)
            pcfComponent.Value?.Find(smc => smc.IdShort == "GoodsHandoverAddress");

        if (goodsHandoverAddress != null)
        {
            // Adresse setzen
            var contactInfo = ContactInformationCreator.CreateFromJson(
                (address?.Name == null && address?.Vorname == null)
                    ? string.Empty
                    : (address?.Name ?? string.Empty) + " " + (address?.Vorname ?? string.Empty),
                address?.Strasse ?? string.Empty,
                address?.Ort ?? string.Empty,
                address?.Plz ?? string.Empty,
                address?.Land ?? string.Empty
            );
            goodsHandoverAddress.Value = contactInfo.Value;
        }
        return pcfComponent;
    }
}
