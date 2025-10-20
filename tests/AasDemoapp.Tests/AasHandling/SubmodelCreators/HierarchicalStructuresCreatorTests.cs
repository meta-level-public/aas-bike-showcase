using System;
using System.IO;
using System.Linq;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling.SubmodelCreators;
using Xunit;

namespace AasDemoapp.Tests.AasHandling.SubmodelCreators;

/// <summary>
/// Unit-Tests für die HierarchicalStructuresCreator Klasse.
/// </summary>
public class HierarchicalStructuresCreatorTests
{
    [Fact]
    public void CreateFromJson_ShouldReturnSubmodel()
    {
        // Act
        var result = HierarchicalStructuresCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Submodel>(result);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveCorrectIdShort()
    {
        // Act
        var result = HierarchicalStructuresCreator.CreateFromJson();

        // Assert
        Assert.Equal("HierarchicalStructures", result.IdShort);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveInstanceKind()
    {
        // Act
        var result = HierarchicalStructuresCreator.CreateFromJson();

        // Assert
        Assert.Equal(ModellingKind.Instance, result.Kind);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveGeneratedId()
    {
        // Act
        var result = HierarchicalStructuresCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result.Id);
        Assert.NotEmpty(result.Id);
        Assert.Contains("https://oi4-nextbike.de", result.Id);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveSemanticId()
    {
        // Act
        var result = HierarchicalStructuresCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result.SemanticId);
        Assert.IsType<Reference>(result.SemanticId);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveSubmodelElements()
    {
        // Act
        var result = HierarchicalStructuresCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result.SubmodelElements);
        Assert.NotEmpty(result.SubmodelElements);
    }

    [Fact]
    public void CreateFromJson_ShouldHaveDescription()
    {
        // Act
        var result = HierarchicalStructuresCreator.CreateFromJson();

        // Assert
        Assert.NotNull(result.Description);
        Assert.NotEmpty(result.Description);
    }

    [Fact]
    public void CreateFromJson_WhenFileNotExists_ShouldThrowException()
    {
        // Arrange - Datei temporär verschieben
        var originalPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "AasHandling",
            "SubmodelCreators",
            "hierarchicalStructures.json"
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
                HierarchicalStructuresCreator.CreateFromJson()
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
