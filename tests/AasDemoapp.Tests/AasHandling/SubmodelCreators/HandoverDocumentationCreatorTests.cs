using System;
using System.IO;
using System.Linq;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling.SubmodelCreators;
using Xunit;

namespace AasDemoapp.Tests.AasHandling.SubmodelCreators;

/// <summary>
/// Unit-Tests für die HandoverDocumentationCreator Klasse.
/// </summary>
public class HandoverDocumentationCreatorTests
{
    private const string TestIdPrefix = "https://oi4-nextbike.de";

    [Fact]
    public void CreateFromJson_ShouldReturnSubmodel()
    {
        // Act
        var result = HandoverDocumentationCreator.CreateFromJson(TestIdPrefix);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Submodel>(result);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveCorrectIdShort()
    {
        // Act
        var result = HandoverDocumentationCreator.CreateFromJson(TestIdPrefix);

        // Assert
        Assert.Equal("HandoverDocumentation", result.IdShort);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveDescription()
    {
        // Act
        var result = HandoverDocumentationCreator.CreateFromJson(TestIdPrefix);

        // Assert
        Assert.NotNull(result.Description);
        Assert.NotEmpty(result.Description);

        var englishDescription = result.Description.FirstOrDefault(d => d.Language == "en");
        Assert.NotNull(englishDescription);
        Assert.Contains("handover of documentation", englishDescription.Text);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveInstanceKind()
    {
        // Act
        var result = HandoverDocumentationCreator.CreateFromJson(TestIdPrefix);

        // Assert
        Assert.Equal(ModellingKind.Instance, result.Kind);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveGeneratedId()
    {
        // Act
        var result = HandoverDocumentationCreator.CreateFromJson(TestIdPrefix);

        // Assert
        Assert.NotNull(result.Id);
        Assert.NotEmpty(result.Id);
        Assert.Contains("https://oi4-nextbike.de", result.Id);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveSemanticId()
    {
        // Act
        var result = HandoverDocumentationCreator.CreateFromJson(TestIdPrefix);

        // Assert
        Assert.NotNull(result.SemanticId);
        Assert.IsType<Reference>(result.SemanticId);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveSubmodelElements()
    {
        // Act
        var result = HandoverDocumentationCreator.CreateFromJson(TestIdPrefix);

        // Assert
        Assert.NotNull(result.SubmodelElements);
        Assert.NotEmpty(result.SubmodelElements);
    }

    [Fact]
    public void CreateFromJson_ShouldContainNumberOfDocumentsElement()
    {
        // Act
        var result = HandoverDocumentationCreator.CreateFromJson(TestIdPrefix);

        // Assert
        var numberOfDocuments = result.SubmodelElements?.FirstOrDefault(e =>
            e.IdShort == "numberOfDocuments"
        );
        Assert.NotNull(numberOfDocuments);
        Assert.Equal("PARAMETER", numberOfDocuments.Category);
    }

    [Fact]
    public void ConvertJsonToSubmodel_WithValidJson_ShouldReturnSubmodel()
    {
        // Arrange
        var validJson = """
            {
                "idShort": "TestSubmodel",
                "id": "https://example.com/test",
                "kind": "Instance",
                "submodelElements": []
            }
            """;

        // Act
        var result = HandoverDocumentationCreator.ConvertJsonToSubmodel(validJson, TestIdPrefix);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestSubmodel", result.IdShort);
        Assert.Equal(ModellingKind.Instance, result.Kind);
    }

    [Fact]
    public void ConvertJsonToSubmodel_WithInvalidJson_ShouldThrowException()
    {
        // Arrange
        var invalidJson = "invalid json";

        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            HandoverDocumentationCreator.ConvertJsonToSubmodel(invalidJson, TestIdPrefix)
        );
    }

    [Fact]
    public void ConvertJsonToSubmodel_WithNullJson_ShouldThrowException()
    {
        // Arrange
        string? nullJson = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            HandoverDocumentationCreator.ConvertJsonToSubmodel(nullJson!, TestIdPrefix)
        );
    }

    [Fact]
    public void CreateFromJson_WhenFileNotExists_ShouldThrowException()
    {
        // Arrange - Datei temporär verschieben
        var originalPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "AasHandling",
            "SubmodelCreators",
            "handoverdoc.json"
        );
        var tempPath = originalPath + ".temp";

        try
        {
            if (System.IO.File.Exists(originalPath))
            {
                System.IO.File.Move(originalPath, tempPath);
            }

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() =>
                HandoverDocumentationCreator.CreateFromJson(TestIdPrefix)
            );
        }
        finally
        {
            // Cleanup
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Move(tempPath, originalPath);
            }
        }
    }
}
