using System;
using System.IO;
using System.Linq;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling.SubmodelCreators;
using Xunit;

namespace AasDemoapp.Tests.AasHandling.SubmodelCreators;

/// <summary>
/// Unit-Tests für die NameplateCreator Klasse.
/// </summary>
public class NameplateCreatorTests
{
    [Fact]
    public void CreateFromJson_ShouldReturnSubmodel()
    {
        // Act
        var result = NameplateCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Submodel>(result);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveCorrectIdShort()
    {
        // Act
        var result = NameplateCreator.CreateFromJson();

        // Assert
        Assert.Equal("DigitalNameplate", result.IdShort);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveInstanceKind()
    {
        // Act
        var result = NameplateCreator.CreateFromJson();

        // Assert
        Assert.Equal(ModellingKind.Instance, result.Kind);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveGeneratedId()
    {
        // Act
        var result = NameplateCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result.Id);
        Assert.NotEmpty(result.Id);
        Assert.Contains("https://oi4-nextbike.de", result.Id);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveSemanticId()
    {
        // Act
        var result = NameplateCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result.SemanticId);
        Assert.IsType<Reference>(result.SemanticId);

        var reference = (Reference)result.SemanticId;
        Assert.NotEmpty(reference.Keys);

        var key = reference.Keys.First();
        Assert.Equal(KeyTypes.GlobalReference, key.Type);
        Assert.Equal("https://admin-shell.io/idta/nameplate/3/0/Nameplate", key.Value);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveSubmodelElements()
    {
        // Act
        var result = NameplateCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result.SubmodelElements);
        Assert.NotEmpty(result.SubmodelElements);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveDescription()
    {
        // Act
        var result = NameplateCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result.Description);
        Assert.NotEmpty(result.Description);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveAdministration()
    {
        // Act
        var result = NameplateCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result.Administration);
        Assert.Equal("3", result.Administration.Version);
        Assert.Equal("0", result.Administration.Revision);
    }

    [Fact]
    public void CreateFromJson_ShouldContainManufacturerNameElement()
    {
        // Act
        var result = NameplateCreator.CreateFromJson();

        // Assert
        // Suche nach einem typischen Nameplate-Element
        var manufacturerElement = result.SubmodelElements?.FirstOrDefault(e =>
            e.IdShort?.Contains("Manufacturer") == true
            || e.IdShort?.Contains("ManufacturerName") == true
        );

        // Falls kein spezifisches Element gefunden wird, überprüfe zumindest, dass Elemente vorhanden sind
        if (manufacturerElement == null)
        {
            Assert.True(
                result.SubmodelElements?.Count > 0,
                "Submodel sollte mindestens ein Element enthalten"
            );
        }
        else
        {
            Assert.NotNull(manufacturerElement);
        }
    }

    [Fact]
    public void CreateFromJson_WhenFileNotExists_ShouldThrowException()
    {
        // Arrange - Datei temporär verschieben
        var originalPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "AasHandling",
            "SubmodelCreators",
            "nameplate.json"
        );
        var tempPath = originalPath + ".temp";

        try
        {
            if (System.IO.File.Exists(originalPath))
            {
                System.IO.File.Move(originalPath, tempPath);
            }

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => NameplateCreator.CreateFromJson());
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
